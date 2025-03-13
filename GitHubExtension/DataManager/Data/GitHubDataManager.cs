// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Client;
using GitHubExtension.Controls;
using GitHubExtension.DataModel;
using GitHubExtension.DataModel.DataObjects;
using GitHubExtension.DataModel.Enums;
using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using Serilog;
using Windows.Storage;

namespace GitHubExtension.DataManager.Data;

public partial class GitHubDataManager : IGitHubDataManager, IPullRequestUpdater, IDataRequester, IDisposable
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", nameof(GitHubDataManager)));

    private static readonly ILogger _log = _logger.Value;

    private const string LastUpdatedKeyName = "LastUpdated";
    private static readonly TimeSpan _searchRetentionTime = TimeSpan.FromDays(7);

    // It is possible different widgets have queries which touch the same pull requests.
    // We want to keep this window large enough that we don't delete data being used by
    // other clients which simply haven't been updated yet but will in the near future.
    // This is a conservative time period to check for pruning and give time for other
    // consumers using the data to update its freshness before we remove it.
    private static readonly TimeSpan _lastObservedDeleteSpan = TimeSpan.FromMinutes(6);

    private DataStore DataStore { get; set; }

    private readonly GitHubClientProvider _gitHubClientProvider;

    public DataStoreOptions DataStoreOptions { get; private set; }

    public GitHubDataManager(GitHubClientProvider gitHubClientProvider, DataStoreOptions? dataStoreOptions = null)
    {
        dataStoreOptions ??= DefaultOptions;

        if (dataStoreOptions.DataStoreSchema == null)
        {
            throw new ArgumentNullException(nameof(dataStoreOptions), "DataStoreSchema cannot be null.");
        }

        DataStoreOptions = dataStoreOptions;

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

        Issue.DeleteLastObservedBefore(DataStore, dsSearch.Id, DateTime.UtcNow - _lastObservedDeleteSpan);
    }

    private async Task UpdatePullRequestsForSearchAsync(string name, string searchString, RequestOptions? options = null)
    {
        _log.Information($"Updating pull requests for: {name} - {searchString}");
        options ??= RequestOptions.RequestOptionsDefault();
        var searchIssuesRequest = new Octokit.SearchIssuesRequest(searchString)
        {
            State = Octokit.ItemState.Open,
            Type = Octokit.IssueTypeQualifier.PullRequest,
            PerPage = 100,
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

            var dsPullRequest = PullRequest.GetOrCreateByOctokitIssue(DataStore, issue);
            SearchPullRequest.AddPullRequestToSearch(DataStore, dsPullRequest, dsSearch);
        }

        PullRequest.DeleteLastObservedBefore(DataStore, dsSearch.Id, DateTime.UtcNow - _lastObservedDeleteSpan);
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
        var cancellationToken = options?.CancellationToken.GetValueOrDefault() ?? default;
        cancellationToken.ThrowIfCancellationRequested();

        switch (type)
        {
            case SearchType.Issues:
                await UpdateIssuesForSearchAsync(name, searchString, options);
                break;
            case SearchType.PullRequests:
                await UpdatePullRequestsForSearchAsync(name, searchString, options);
                break;
            case SearchType.IssuesAndPullRequests:
                await UpdateIssuesForSearchAsync(name, searchString, options);
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

    // Updates a Pull Request using the Pull Request API. This is intended to update the
    // data of a pull request that was created using the Search Issues API.
    public async Task<IPullRequest> UpdatePullRequestFromPullRequestAPIAsync(IPullRequest pullRequest)
    {
        var url = pullRequest.HtmlUrl;

        var owner = url.Split('/')[3];
        var repoName = url.Split('/')[4];

        var client = await _gitHubClientProvider.GetClientForLoggedInDeveloper(true);
        var octokitPullRequest = await client.PullRequest.Get(owner, repoName, (int)pullRequest.Number);
        return PullRequest.GetOrCreateByOctokitPullRequest(DataStore, octokitPullRequest);
    }

    public Search? GetSearch(string name, string searchString)
    {
        ValidateDataStore();
        return Search.Get(DataStore, name, searchString);
    }

    public IEnumerable<Issue> GetIssuesForSearch(string name, string searchString)
    {
        ValidateDataStore();
        return GetSearch(name, searchString)?.Issues ?? [];
    }

    public IEnumerable<PullRequest> GetPullRequestsForSearch(string name, string searchString)
    {
        ValidateDataStore();
        return GetSearch(name, searchString)?.PullRequests ?? [];
    }

    // Removes unused data from the datastore.
    private void PruneObsoleteData()
    {
        Search.DeleteBefore(DataStore, DateTime.UtcNow - _searchRetentionTime);
        SearchIssue.DeleteUnreferenced(DataStore);
        SearchPullRequest.DeleteUnreferenced(DataStore);
        SearchRepository.DeleteUnreferenced(DataStore);
    }

    // Sets a last-updated in the MetaData.
    private void SetLastUpdatedInMetaData()
    {
        MetaData.AddOrUpdate(DataStore, LastUpdatedKeyName, DateTime.Now.ToDataStoreString());
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
