// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Client;
using GitHubExtension.DataManager;
using GitHubExtension.DataModel;
using GitHubExtension.DataModel.Enums;
using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using GitHubExtension.Pages;
using Serilog;
using Windows.Storage;

namespace GitHubExtension;

public partial class GitHubDataManager : IGitHubDataManager, IDisposable
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", nameof(GitHubDataManager)));

    private static readonly ILogger _log = _logger.Value;

    private const string LastUpdatedKeyName = "LastUpdated";
    private static readonly TimeSpan _notificationRetentionTime = TimeSpan.FromDays(7);
    private static readonly TimeSpan _searchRetentionTime = TimeSpan.FromDays(7);
    private static readonly TimeSpan _pullRequestStaleTime = TimeSpan.FromDays(1);
    private static readonly TimeSpan _reviewStaleTime = TimeSpan.FromDays(7);
    private static readonly TimeSpan _releaseRetentionTime = TimeSpan.FromDays(7);

    // It is possible different widgets have queries which touch the same pull requests.
    // We want to keep this window large enough that we don't delete data being used by
    // other clients which simply haven't been updated yet but will in the near future.
    // This is a conservative time period to check for pruning and give time for other
    // consumers using the data to update its freshness before we remove it.
    private static readonly TimeSpan _lastObservedDeleteSpan = TimeSpan.FromMinutes(6);
    private const long CheckSuiteIdDependabot = 29110;

    private DataStore DataStore { get; set; }

    private readonly IDeveloperIdProvider _developerIdProvider;
    private readonly GitHubClientProvider _gitHubClientProvider;

    public DataStoreOptions DataStoreOptions { get; private set; }

    public GitHubDataManager(IDeveloperIdProvider developerIdProvider, GitHubClientProvider gitHubClientProvider, DataStoreOptions? dataStoreOptions = null)
    {
        dataStoreOptions ??= DefaultOptions;

        if (dataStoreOptions.DataStoreSchema == null)
        {
            throw new ArgumentNullException(nameof(dataStoreOptions), "DataStoreSchema cannot be null.");
        }

        DataStoreOptions = dataStoreOptions;

        _developerIdProvider = developerIdProvider;
        _gitHubClientProvider = gitHubClientProvider;

        DataStore = new DataStore(
            "DataStore",
            Path.Combine(dataStoreOptions.DataStoreFolderPath, dataStoreOptions.DataStoreFileName),
            dataStoreOptions.DataStoreSchema);
        DataStore.Create();
    }

    public DateTime LastUpdated
    {
        get
        {
            ValidateDataStore();
            var lastUpdated = MetaData.Get(DataStore, LastUpdatedKeyName);
            if (lastUpdated == null)
            {
                return DateTime.MinValue;
            }

            return lastUpdated.ToDateTime();
        }

        set
        {
            ValidateDataStore();
            MetaData.AddOrUpdate(DataStore, LastUpdatedKeyName, value.ToDataStoreString());
        }
    }

    public async Task UpdateAllDataForRepositoryAsync(string owner, string name, RequestOptions? options = null)
    {
        ValidateDataStore();
        var parameters = new DataStoreOperationParameters
        {
            Owner = owner,
            RepositoryName = name,
            RequestOptions = options,
            OperationName = "UpdateAllDataForRepositoryAsync",
        };

        await UpdateDataForRepositoryAsync(
            parameters,
            async (parameters, devId) =>
            {
                var repository = await UpdateRepositoryAsync(parameters.Owner!, parameters.RepositoryName!, devId.GitHubClient);
                await UpdateIssuesAsync(repository, devId.GitHubClient, parameters.RequestOptions);
                await UpdatePullRequestsAsync(repository, devId.GitHubClient, parameters.RequestOptions);
            });
    }

    public async Task UpdateAllDataForRepositoryAsync(string fullName, RequestOptions? options = null)
    {
        ValidateDataStore();
        var nameSplit = GetOwnerAndRepositoryNameFromFullName(fullName);
        await UpdateAllDataForRepositoryAsync(nameSplit[0], nameSplit[1], options);
    }

    public async Task UpdateAllDataForRepositoriesAsync(Octokit.RepositoryCollection repoCollection, RequestOptions requestOptions)
    {
        ValidateDataStore();
        var cancellationToken = requestOptions?.CancellationToken.GetValueOrDefault() ?? default;
        foreach (var repo in repoCollection)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await UpdateAllDataForRepositoryAsync(repo, requestOptions);
        }
    }

    public async Task UpdatePullRequestsForRepositoryAsync(string owner, string name, RequestOptions? options = null)
    {
        ValidateDataStore();
        var parameters = new DataStoreOperationParameters
        {
            Owner = owner,
            RepositoryName = name,
            RequestOptions = options,
            OperationName = "UpdatePullRequestsForRepositoryAsync",
        };

        await UpdateDataForRepositoryAsync(
            parameters,
            async (parameters, devId) =>
            {
                var repository = await UpdateRepositoryAsync(parameters.Owner!, parameters.RepositoryName!, devId.GitHubClient);
                await UpdatePullRequestsAsync(repository, devId.GitHubClient, parameters.RequestOptions);
            });
    }

    public async Task UpdatePullRequestsForRepositoryAsync(string fullName, RequestOptions? options = null)
    {
        ValidateDataStore();
        var nameSplit = GetOwnerAndRepositoryNameFromFullName(fullName);
        await UpdatePullRequestsForRepositoryAsync(nameSplit[0], nameSplit[1], options);
    }

    public async Task UpdatePullRequestsForRepositoriesAsync(Octokit.RepositoryCollection repoCollection, RequestOptions requestOptions)
    {
        ValidateDataStore();
        foreach (var repo in repoCollection)
        {
            await UpdatePullRequestsForRepositoryAsync(repo, requestOptions);
        }
    }

    public async Task UpdateIssuesForRepositoryAsync(string owner, string name, RequestOptions? options = null)
    {
        ValidateDataStore();
        var parameters = new DataStoreOperationParameters
        {
            Owner = owner,
            RepositoryName = name,
            RequestOptions = options,
            OperationName = "UpdateIssuesForRepositoryAsync",
        };

        await UpdateDataForRepositoryAsync(
            parameters,
            async (parameters, devId) =>
            {
                var repository = await UpdateRepositoryAsync(parameters.Owner!, parameters.RepositoryName!, devId.GitHubClient);
                await UpdateIssuesAsync(repository, devId.GitHubClient, parameters.RequestOptions);
            });
    }

    public async Task UpdateIssuesForRepositoryAsync(string fullName, RequestOptions? options = null)
    {
        ValidateDataStore();
        var nameSplit = GetOwnerAndRepositoryNameFromFullName(fullName);
        await UpdateIssuesForRepositoryAsync(nameSplit[0], nameSplit[1], options);
    }

    public async Task UpdateIssuesForRepositoriesAsync(Octokit.RepositoryCollection repoCollection, RequestOptions requestOptions)
    {
        ValidateDataStore();
        foreach (var repo in repoCollection)
        {
            await UpdateIssuesForRepositoryAsync(repo, requestOptions);
        }
    }

    public IEnumerable<Repository> GetRepositories()
    {
        ValidateDataStore();
        return Repository.GetAll(DataStore);
    }

    public Repository? GetRepository(string owner, string name)
    {
        ValidateDataStore();
        return Repository.Get(DataStore, owner, name);
    }

    public Repository? GetRepository(string fullName)
    {
        ValidateDataStore();
        return Repository.Get(DataStore, fullName);
    }

    // Wrapper for the targeted repository update pattern.
    // This is where we are querying specific data.
    private async Task UpdateDataForRepositoryAsync(DataStoreOperationParameters parameters, Func<DataStoreOperationParameters, DeveloperId.DeveloperId, Task> asyncAction)
    {
        parameters.RequestOptions ??= RequestOptions.RequestOptionsDefault();
        parameters.DeveloperIds = _developerIdProvider.GetLoggedInDeveloperIdsInternal();

        ValidateRepositoryOwnerAndName(parameters.Owner!, parameters.RepositoryName!);
        if (parameters.RequestOptions.UsePublicClientAsFallback)
        {
            // Append the public client to the list of developer accounts. This will have us try the public client as a fallback.
            parameters.DeveloperIds = parameters.DeveloperIds.Concat(new[] { new DeveloperId.DeveloperId() });
        }

        var cancellationToken = parameters.RequestOptions?.CancellationToken.GetValueOrDefault() ?? default;

        cancellationToken.ThrowIfCancellationRequested();
        var found = false;

        // We only need to get the information from one account which has access.
        foreach (var devId in parameters.DeveloperIds)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Try the action for the passed in developer Id.
                await asyncAction(parameters, _developerIdProvider.GetDeveloperIdInternal(devId));

                // We can stop when the action is executed without exceptions.
                found = true;
                break;
            }
            catch (Exception ex) when (ex is Octokit.ApiException)
            {
                switch (ex)
                {
                    case Octokit.NotFoundException:
                        // A private repository will come back as "not found" by the GitHub API when an unauthorized account cannot even view it.
                        _log.Debug($"DeveloperId {devId.LoginId} did not find {parameters.Owner}/{parameters.RepositoryName}");
                        continue;

                    case Octokit.RateLimitExceededException:
                        _log.Debug($"DeveloperId {devId.LoginId} rate limit exceeded.");
                        throw;

                    case Octokit.ForbiddenException:
                        // This can happen most commonly with SAML-enabled organizations.
                        // The user may have access but the org blocked the application.
                        _log.Debug($"DeveloperId {devId.LoginId} was forbidden access to {parameters.Owner}/{parameters.RepositoryName}");
                        continue;

                    default:
                        // If it's some other error like abuse detection, abort and do not continue.
                        _log.Debug($"Unhandled Octokit API error for {devId.LoginId} and {parameters.Owner} / {parameters.RepositoryName}");
                        continue;
                }
            }
        }

        if (!found)
        {
            // We choose to not throw here so we can
            // get the other repositories.
            _log.Error($"The repository {parameters.Owner}/{parameters.RepositoryName} could not be accessed by any available developer accounts.");
        }

        _log.Information($"Updated datastore: {parameters}");
    }

    // Internal method to update a repository.
    // DataStore transaction is assumed to be wrapped around this in the public method.
    private async Task<Repository> UpdateRepositoryAsync(string owner, string repositoryName, Octokit.GitHubClient? client = null)
    {
        client ??= await _gitHubClientProvider.GetClientForLoggedInDeveloper(true);
        _log.Information($"Updating repository: {owner}/{repositoryName}");
        var octokitRepository = await client.Repository.Get(owner, repositoryName);
        return Repository.GetOrCreateByOctokitRepository(DataStore, octokitRepository);
    }

    // Internal method to update pull requests. Assumes Repository has already been populated and
    // created. DataStore transaction is assumed to be wrapped around this in the public method.
    private async Task UpdatePullRequestsAsync(Repository repository, Octokit.GitHubClient? client = null, RequestOptions? options = null)
    {
        options ??= RequestOptions.RequestOptionsDefault();
        client ??= await _gitHubClientProvider.GetClientForLoggedInDeveloper(true);
        var user = await client.User.Current();
        _log.Information($"Updating pull requests for: {repository.FullName} and user: {user.Login}");
        var octoPulls = await client.PullRequest.GetAllForRepository(repository.InternalId, options.PullRequestRequest, options.ApiOptions);
        _log.Information($"Got {octoPulls.Count} pull requests.");

        var cancellationToken = options?.CancellationToken.GetValueOrDefault() ?? default;

        foreach (var pull in octoPulls)
        {
            cancellationToken.ThrowIfCancellationRequested();

            PullRequest.GetOrCreateByOctokitPullRequest(DataStore, pull, repository.Id);
        }

        // Remove unobserved pull requests from this repository.
        PullRequest.DeleteLastObservedBefore(DataStore, repository.Id, DateTime.UtcNow - _lastObservedDeleteSpan);
    }

    // Internal method to update issues. Assumes Repository has already been populated and created.
    // DataStore transaction is assumed to be wrapped around this in the public method.
    private async Task UpdateIssuesAsync(Repository repository, Octokit.GitHubClient? client = null, RequestOptions? options = null)
    {
        options ??= RequestOptions.RequestOptionsDefault();
        client ??= await _gitHubClientProvider.GetClientForLoggedInDeveloper(true);
        _log.Information($"Updating issues for: {repository.FullName}");

        // Since we are only interested in issues and for a specific repository, we will override
        // these two properties. All other properties the caller can specify however they see fit.
        options.SearchIssuesRequest.Type = Octokit.IssueTypeQualifier.Issue;
        options.SearchIssuesRequest.Repos = new Octokit.RepositoryCollection { repository.FullName };

        var issuesResult = await client.Search.SearchIssues(options.SearchIssuesRequest);

        if (issuesResult == null)
        {
            _log.Debug($"No issues found.");
            return;
        }

        var cancellationToken = options?.CancellationToken.GetValueOrDefault() ?? default;

        _log.Debug($"Results contain {issuesResult.Items.Count} issues.");
        foreach (var issue in issuesResult.Items)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Issue.GetOrCreateByOctokitIssue(DataStore, issue, repository.Id);
        }

        // Remove issues from this repository that were not observed recently.
        Issue.DeleteLastObservedBefore(DataStore, repository.Id, DateTime.UtcNow - _lastObservedDeleteSpan);
    }

    // Search area
    // Methods to update Search items
    private async Task UpdateIssuesForSearchAsync(string name, string searchString, RequestOptions? options = null)
    {
        _log.Information($"Updating issues for: {name} - {searchString}");
        options ??= RequestOptions.RequestOptionsDefault();
        var searchIssuesRequest = new Octokit.SearchIssuesRequest(searchString)
        {
            State = Octokit.ItemState.Open,
            Type = Octokit.IssueTypeQualifier.Issue,
        };
        var issuesResult = await _gitHubClientProvider.GetClient().Search.SearchIssues(searchIssuesRequest);
        if (issuesResult == null)
        {
            _log.Information($"No issues found.");
            return;
        }

        _log.Information($"Results contain {issuesResult.Items.Count} issues.");

        var cancellationToken = options?.CancellationToken.GetValueOrDefault() ?? default;
        var dsSearch = Search.GetOrCreate(DataStore, name, searchString);

        foreach (var issue in issuesResult.Items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var dsIssue = Issue.GetOrCreateByOctokitIssue(DataStore, issue);
            SearchIssue.AddIssueToSearch(DataStore, dsIssue, dsSearch);
        }
    }

    private async Task UpdatePullRequestsForSearchAsync(string name, string searchString, RequestOptions? options = null)
    {
        _log.Information($"Updating pull requests for: {name} - {searchString}");
        options ??= RequestOptions.RequestOptionsDefault();
        var searchIssuesRequest = new Octokit.SearchIssuesRequest(searchString)
        {
            State = Octokit.ItemState.Open,
            Type = Octokit.IssueTypeQualifier.PullRequest,
            PerPage = 10,
        };
        var issuesResult = await _gitHubClientProvider.GetClient().Search.SearchIssues(searchIssuesRequest);
        if (issuesResult == null)
        {
            _log.Information($"No pull requests found.");
            return;
        }

        _log.Information($"Results contain {issuesResult.Items.Count} pull requests.");

        var cancellationToken = options?.CancellationToken.GetValueOrDefault() ?? default;
        var dsSearch = Search.GetOrCreate(DataStore, name, searchString);

        foreach (var issue in issuesResult.Items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var issueUrl = issue.Url;

            var owner = issueUrl.Split('/')[4];
            var repoName = issueUrl.Split('/')[5];

            var pullRequest = await _gitHubClientProvider.GetClient().PullRequest.Get(owner, repoName, issue.Number);
            var dsPullRequest = PullRequest.GetOrCreateByOctokitPullRequest(DataStore, pullRequest);
            SearchPullRequest.AddPullRequestToSearch(DataStore, dsPullRequest, dsSearch);
        }
    }

    private async Task UpdateRepositoriesForSearchAsync(string name, string searchString, RequestOptions? options = null)
    {
        _log.Information($"Updating repositories for: {name}");
        options ??= RequestOptions.RequestOptionsDefault();
        var searchRepoRequest = new Octokit.SearchRepositoriesRequest(searchString);
        var reposResult = await _gitHubClientProvider.GetClient().Search.SearchRepo(searchRepoRequest);

        if (reposResult == null)
        {
            _log.Debug($"No repositories found.");
            return;
        }

        var cancellationToken = options?.CancellationToken.GetValueOrDefault() ?? default;
        _log.Debug($"Results contain {reposResult.Items.Count} repositories.");
        var dsSearch = Search.GetOrCreate(DataStore, name, searchString);
        foreach (var repo in reposResult.Items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var dsRepository = Repository.GetOrCreateByOctokitRepository(DataStore, repo);
            SearchRepository.AddRepositoryToSearch(DataStore, dsRepository, dsSearch);
        }
    }

    public async Task UpdateDataForSearchAsync(string name, string searchString, SearchType type, RequestOptions options)
    {
        var cancellaTionToken = options?.CancellationToken.GetValueOrDefault() ?? default;
        cancellaTionToken.ThrowIfCancellationRequested();

        switch (type)
        {
            case SearchType.Issues:
                await UpdateIssuesForSearchAsync(name, searchString, options);
                break;
            case SearchType.PullRequests:
                await UpdatePullRequestsForSearchAsync(name, searchString, options);
                break;
            case SearchType.Repositories:
                await UpdateRepositoriesForSearchAsync(name, searchString, options);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public async Task UpdateDataForSearchesAsync(IEnumerable<ISearch> searches, RequestOptions options)
    {
        foreach (var search in searches)
        {
            await UpdateDataForSearchAsync(search.Name, search.SearchString, search.Type, options);
        }
    }

    public Search? GetSearch(string name, string searchString)
    {
        ValidateDataStore();
        return Search.Get(DataStore, name, searchString);
    }

    // Removes unused data from the datastore.
    private void PruneObsoleteData()
    {
        Search.DeleteBefore(DataStore, DateTime.Now - _searchRetentionTime);
        SearchIssue.DeleteUnreferenced(DataStore);
        SearchRepository.DeleteUnreferenced(DataStore);
    }

    // Sets a last-updated in the MetaData.
    private void SetLastUpdatedInMetaData()
    {
        MetaData.AddOrUpdate(DataStore, LastUpdatedKeyName, DateTime.Now.ToDataStoreString());
    }

    // Converts fullName -> owner, name.
    private string[] GetOwnerAndRepositoryNameFromFullName(string fullName)
    {
        var nameSplit = fullName.Split(['/']);
        if (nameSplit.Length != 2 || string.IsNullOrEmpty(nameSplit[0]) || string.IsNullOrEmpty(nameSplit[1]))
        {
            _log.Error($"Invalid repository full name: {fullName}");
            throw new ArgumentException($"Invalid repository full name: {fullName}");
        }

        return nameSplit;
    }

    private string GetFullNameFromOwnerAndRepository(string owner, string repository)
    {
        return $"{owner}/{repository}";
    }

    private void ValidateRepositoryOwnerAndName(string owner, string repositoryName)
    {
        if (string.IsNullOrEmpty(owner))
        {
            throw new ArgumentNullException(nameof(owner));
        }

        if (string.IsNullOrEmpty(repositoryName))
        {
            throw new ArgumentNullException(nameof(repositoryName));
        }
    }

    private void ValidateDataStore()
    {
        if (DataStore is null || !DataStore.IsConnected)
        {
            throw new DataStoreInaccessibleException("DataStore is not available.");
        }
    }

    // Making the default options a singleton to avoid repeatedly calling the storage APIs and
    // creating a new GitHubDataStoreSchema when not necessary.
    private static readonly Lazy<DataStoreOptions> _lazyDataStoreOptions = new(DefaultOptionsInit);

    private static DataStoreOptions DefaultOptions => _lazyDataStoreOptions.Value;

    private static DataStoreOptions DefaultOptionsInit()
    {
        return new DataStoreOptions
        {
            DataStoreFolderPath = ApplicationData.Current.LocalFolder.Path,
            DataStoreSchema = new GitHubDataStoreSchema(),
        };
    }

    public override string ToString() => "GitHubDataManager";

    private bool _disposed; // To detect redundant calls.

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _log.Debug("Disposing of all Disposable resources.");

            if (disposing)
            {
                if (DataStore != null)
                {
                    try
                    {
                        DataStore.Dispose();
                    }
                    catch
                    {
                    }
                }
            }

            _disposed = true;
        }
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private static bool IsCancelException(Exception ex)
    {
        return (ex is OperationCanceledException) || (ex is TaskCanceledException);
    }
}
