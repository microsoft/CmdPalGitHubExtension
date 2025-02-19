// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Client;
using GitHubExtension.DataManager;
using GitHubExtension.DataModel;
using GitHubExtension.DataModel.Enums;
using GitHubExtension.Helpers;
using Serilog;
using Windows.Storage;

namespace GitHubExtension;

public partial class GitHubDataManager : IGitHubDataManager, IDisposable
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", nameof(GitHubDataManager)));

    private static readonly ILogger _log = _logger.Value;

    public static event DataManagerUpdateEventHandler? OnUpdate;

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

    public DataStoreOptions DataStoreOptions { get; private set; }

    public static IGitHubDataManager? CreateInstance(DataStoreOptions? options = null)
    {
        options ??= DefaultOptions;

        try
        {
            return new GitHubDataManager(options);
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed creating GitHubDataManager");
            Environment.FailFast(e.Message, e);
            return null;
        }
    }

    public GitHubDataManager(DataStoreOptions dataStoreOptions)
    {
        if (dataStoreOptions.DataStoreSchema == null)
        {
            throw new ArgumentNullException(nameof(dataStoreOptions), "DataStoreSchema cannot be null.");
        }

        DataStoreOptions = dataStoreOptions;

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
        foreach (var repo in repoCollection)
        {
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

    public async Task UpdatePullRequestsForLoggedInDeveloperIdsAsync()
    {
        ValidateDataStore();
        var parameters = new DataStoreOperationParameters
        {
            OperationName = "UpdatePullRequestsForLoggedInDeveloperIdsAsync",
        };
        await UpdateDataStoreAsync(parameters, UpdatePullRequestsForLoggedInDeveloperIdsAsync);
    }

    public async Task UpdateReleasesForRepositoryAsync(string owner, string name, RequestOptions? options = null)
    {
        ValidateDataStore();
        var parameters = new DataStoreOperationParameters
        {
            Owner = owner,
            RepositoryName = name,
            RequestOptions = options,
            OperationName = "UpdateReleasesForRepositoryAsync",
        };

        await UpdateDataForRepositoryAsync(
            parameters,
            async (parameters, devId) =>
            {
                var repository = await UpdateRepositoryAsync(parameters.Owner!, parameters.RepositoryName!, devId.GitHubClient);
                await UpdateReleasesAsync(repository, devId.GitHubClient, parameters.RequestOptions);
            });
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

    public IEnumerable<User> GetDeveloperUsers()
    {
        ValidateDataStore();
        return User.GetDeveloperUsers(DataStore);
    }

    public IEnumerable<Notification> GetNotifications(DateTime? since = null, bool includeToasted = false)
    {
        ValidateDataStore();
        return Notification.Get(DataStore, since, includeToasted);
    }

    // Wrapper for database operations for consistent handling.
    private async Task UpdateDataStoreAsync(DataStoreOperationParameters parameters, Func<DataStoreOperationParameters, Task> asyncAction)
    {
        parameters.RequestOptions ??= RequestOptions.RequestOptionsDefault();
        parameters.DeveloperIds = DeveloperId.DeveloperIdProvider.GetInstance().GetLoggedInDeveloperIdsInternal();
        using var tx = DataStore.Connection!.BeginTransaction();

        try
        {
            // Do the action on the repository for the client.
            await asyncAction(parameters);

            // Clean datastore and set last updated after updating.
            PruneObsoleteData();
            SetLastUpdatedInMetaData();
        }
        catch (HttpRequestException httpEx)
        {
            // HttpRequestExceptions can happen when internet connection is
            // down or various other network issues.
            _log.Warning($"Http Request Exception: {httpEx.Message}");
            tx.Rollback();

            // Rethrow so clients can catch/display appropriate UX.
            throw;
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"Failed Updating DataStore for: {parameters}");
            tx.Rollback();

            // Rethrow so clients can catch/display an error UX.
            throw;
        }

        tx.Commit();
        _log.Information($"Updated datastore: {parameters}");
    }

    // Wrapper for the targeted repository update pattern.
    // This is where we are querying specific data.
    private async Task UpdateDataForRepositoryAsync(DataStoreOperationParameters parameters, Func<DataStoreOperationParameters, DeveloperId.DeveloperId, Task> asyncAction)
    {
        parameters.RequestOptions ??= RequestOptions.RequestOptionsDefault();
        parameters.DeveloperIds = DeveloperId.DeveloperIdProvider.GetInstance().GetLoggedInDeveloperIdsInternal();

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
                await asyncAction(parameters, DeveloperId.DeveloperIdProvider.GetInstance().GetDeveloperIdInternal(devId));

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

    // Find all pull requests the developer, and update them.
    // This is intended to be called from within InternalUpdateDataStoreAsync.
    private async Task UpdatePullRequestsForLoggedInDeveloperIdsAsync(DataStoreOperationParameters parameters)
    {
        var devIds = DeveloperId.DeveloperIdProvider.GetInstance().GetLoggedInDeveloperIdsInternal();
        if (devIds is null || !devIds.Any())
        {
            // This may not be an error if the user has not yet logged in with a DevId.
            _log.Information("Called to update all pull requests for a user with no logged in developer.");
            return;
        }

        // Get pull requests for each logged in developer Id.
        foreach (var devId in devIds)
        {
            // Get the list of all of a user's open pull requests for the logged in developer.
            var searchIssuesRequest = new Octokit.SearchIssuesRequest()
            {
                Author = devId.LoginId,
                State = Octokit.ItemState.Open,
                Type = Octokit.IssueTypeQualifier.PullRequest,
            };

            var searchResult = await devId.GitHubClient.Search.SearchIssues(searchIssuesRequest);
            var repositoryDict = new Dictionary<string, bool>();
            foreach (var issue in searchResult.Items)
            {
                // The Issue search result does not give us enough information to collect
                // what we need about pull requests. So, instead of trying to convert
                // these results into incomplete data objects, we will get the repositories
                // to which these pull requests belong, and then do a Repository pull
                // request query which will have all of the information needed.
                repositoryDict[Validation.ParseFullNameFromGitHubURL(issue.HtmlUrl)] = true;
            }

            // Set request options to get all open PRs for this user.
            var requestOptions = new RequestOptions
            {
                PullRequestRequest = new Octokit.PullRequestRequest
                {
                    State = Octokit.ItemStateFilter.Open,
                },
                ApiOptions = new Octokit.ApiOptions
                {
                    // Use default, which will get all.
                },
            };

            foreach (var repoFullName in repositoryDict.Keys)
            {
                var repoName = GetOwnerAndRepositoryNameFromFullName(repoFullName);
                Octokit.Repository octoRepository;
                try
                {
                    octoRepository = await devId.GitHubClient.Repository.Get(repoName[0], repoName[1]);
                }
                catch (Octokit.ForbiddenException ex)
                {
                    // The list of pull requests produced from the issues search does not account
                    // for SAML enforcement. Skip this repository if Forbidden. This may be
                    // opportunity to prompt user to fix the issue, but not if it is a background
                    // update. Consider placing these errors in an AccessDenied table and allowing
                    // the UI to query it to attempt to resolve any issues discovered in the main
                    // app UX, such as via a user prompt that tells them not all of their data
                    // could be accessed.
                    _log.Warning($"Forbidden exception while trying to query repository {repoFullName}: {ex.Message}");
                    continue;
                }

                var dsRepository = Repository.GetOrCreateByOctokitRepository(DataStore, octoRepository);
                var octoPullRequests = await devId.GitHubClient.PullRequest.GetAllForRepository(repoName[0], repoName[1], requestOptions.PullRequestRequest, requestOptions.ApiOptions);
                foreach (var octoPull in octoPullRequests)
                {
                    // We only care about pulls where the user is the Author.
                    if (octoPull.User.Login != devId.LoginId)
                    {
                        continue;
                    }

                    // Update this pull request and associated CheckRuns and CheckSuites.
                    var dsPullRequest = PullRequest.GetOrCreateByOctokitPullRequest(DataStore, octoPull, dsRepository.Id);

                    try
                    {
                        // Remove all current information about the pull request first, so if there is an
                        // error retrieving the updated information, we do not have torn state and would
                        // instead have no state for it.
                        CheckRun.DeleteAllForPullRequest(DataStore, dsPullRequest);
                        CheckSuite.DeleteAllForPullRequest(DataStore, dsPullRequest);

                        var octoCheckRunResponse = await devId.GitHubClient.Check.Run.GetAllForReference(repoName[0], repoName[1], dsPullRequest.HeadSha);
                        foreach (var run in octoCheckRunResponse.CheckRuns)
                        {
                            CheckRun.GetOrCreateByOctokitCheckRun(DataStore, run);
                        }

                        var octoCheckSuiteResponse = await devId.GitHubClient.Check.Suite.GetAllForReference(repoName[0], repoName[1], dsPullRequest.HeadSha);
                        foreach (var suite in octoCheckSuiteResponse.CheckSuites)
                        {
                            // Skip Dependabot, as it is not part of a pull request's blocking suites.
                            if (suite.App.Id == CheckSuiteIdDependabot)
                            {
                                continue;
                            }

                            _log.Verbose($"Suite: {suite.App.Name} - {suite.App.Id} - {suite.App.Owner.Login}  Conclusion: {suite.Conclusion}  Status: {suite.Status}");
                            CheckSuite.GetOrCreateByOctokitCheckSuite(DataStore, suite);
                        }
                    }
                    catch (Exception e)
                    {
                        // Octokit can sometimes fail unexpectedly or have bugs. Should that occur here, we
                        // will not stop processing all pull requests and instead skip  over getting the PR
                        // checks information for this particular pull request.
                        _log.Error($"Error updating Check Status for Pull Request #{octoPull.Number}: {e.Message}");

                        // Put the full stack trace in debug if this occurs to reduce log spam.
                        _log.Debug(e, $"Error updating Check Status for Pull Request #{octoPull.Number}.");
                    }

                    var commitCombinedStatus = await devId.GitHubClient.Repository.Status.GetCombined(dsRepository.InternalId, dsPullRequest.HeadSha);
                    CommitCombinedStatus.GetOrCreate(DataStore, commitCombinedStatus);

                    CreatePullRequestStatus(dsPullRequest);

                    // Review information for this pull request.
                    // We will only get review data for the logged-in Developer's pull requests.
                    try
                    {
                        var octoReviews = await devId.GitHubClient.PullRequest.Review.GetAll(repoName[0], repoName[1], octoPull.Number);
                        foreach (var octoReview in octoReviews)
                        {
                            ProcessReview(dsPullRequest, octoReview);
                        }
                    }
                    catch (Exception e)
                    {
                        // Octokit can sometimes fail unexpectedly or have bugs. Should that occur here, we
                        // will not stop processing all pull requests and instead skip  over getting the PR
                        // review information for this particular pull request.
                        _log.Error($"Error updating Reviews for Pull Request #{octoPull.Number}: {e.Message}");

                        // Put the full stack trace in debug if this occurs to reduce log spam.
                        _log.Debug(e, $"Error updating Reviews for Pull Request #{octoPull.Number}.");
                    }
                }

                _log.Debug($"Updated developer pull requests for {repoFullName}.");
            }

            // After we update for this developer, remove all pull requests for this developer that
            // were not observed recently.
            PullRequest.DeleteAllByDeveloperLoginAndLastObservedBefore(DataStore, devId.LoginId, DateTime.UtcNow - _lastObservedDeleteSpan);
        }
    }

    // Internal method to update a repository.
    // DataStore transaction is assumed to be wrapped around this in the public method.
    private async Task<Repository> UpdateRepositoryAsync(string owner, string repositoryName, Octokit.GitHubClient? client = null)
    {
        client ??= await GitHubClientProvider.Instance.GetClientForLoggedInDeveloper(true);
        _log.Information($"Updating repository: {owner}/{repositoryName}");
        var octokitRepository = await client.Repository.Get(owner, repositoryName);
        return Repository.GetOrCreateByOctokitRepository(DataStore, octokitRepository);
    }

    // Internal method to update pull requests. Assumes Repository has already been populated and
    // created. DataStore transaction is assumed to be wrapped around this in the public method.
    private async Task UpdatePullRequestsAsync(Repository repository, Octokit.GitHubClient? client = null, RequestOptions? options = null)
    {
        options ??= RequestOptions.RequestOptionsDefault();
        client ??= await GitHubClientProvider.Instance.GetClientForLoggedInDeveloper(true);
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

    private void ProcessReview(PullRequest pullRequest, Octokit.PullRequestReview octoReview)
    {
        // Skip reviews that are stale.
        if ((DateTime.Now - octoReview.SubmittedAt) > _reviewStaleTime)
        {
            return;
        }

        // For creating review notifications, must first determine if the review has changed.
        var existingReview = Review.GetByInternalId(DataStore, octoReview.Id);

        // Add/update the review record.
        var newReview = Review.GetOrCreateByOctokitReview(DataStore, octoReview, pullRequest.Id);

        // Ignore comments or pending state.
        if (string.IsNullOrEmpty(newReview.State) || newReview.State == "Commented")
        {
            _log.Debug($"Ignoring review for {pullRequest}. State: {newReview.State}");
            return;
        }

        // Create a new notification if the state is different or the review did not exist.
        if (existingReview == null || (existingReview.State != newReview.State))
        {
            // We assume that the logged in developer created this pull request.
            _log.Information($"Creating NewReview Notification for {pullRequest}. State: {newReview.State}");
            Notification.Create(DataStore, newReview, NotificationType.NewReview);
        }
    }

    private void CreatePullRequestStatus(PullRequest pullRequest)
    {
        // Get the previous status for comparison.
        var prevStatus = PullRequestStatus.Get(DataStore, pullRequest);

        // Create the new status.
        var curStatus = PullRequestStatus.Add(DataStore, pullRequest);

        if (ShouldCreateCheckFailureNotification(curStatus, prevStatus))
        {
            _log.Information($"Creating CheckRunFailure Notification for {curStatus}");
            Notification.Create(DataStore, curStatus, NotificationType.CheckRunFailed);
        }

        if (ShouldCreateCheckSucceededNotification(curStatus, prevStatus))
        {
            _log.Debug($"Creating CheckRunSuccess Notification for {curStatus}");
            Notification.Create(DataStore, curStatus, NotificationType.CheckRunSucceeded);
        }
    }

    public bool ShouldCreateCheckFailureNotification(PullRequestStatus curStatus, PullRequestStatus? prevStatus)
    {
        // If the pull request is not recent, do not create a notification for it.
        if ((DateTime.Now - curStatus.PullRequest.UpdatedAt) > _pullRequestStaleTime)
        {
            return false;
        }

        // Compare pull request status.
        if (prevStatus is null || prevStatus.HeadSha != curStatus.HeadSha)
        {
            // No previous status for this commit, assume new PR or freshly pushed commit with
            // checks likely running. Any check failures here are assumed to be notification worthy.
            if (curStatus.Failed)
            {
                return true;
            }
        }
        else
        {
            // A failure isn't necessarily notification worthy if we've already seen it.
            if (curStatus.Failed)
            {
                // If the previous status was not failed, or the failure was for a different
                // reason (example, it moved from ActionRequired -> Failure), that is
                // notification worthy.
                if (!prevStatus.Failed || curStatus.Conclusion != prevStatus.Conclusion)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool ShouldCreateCheckSucceededNotification(PullRequestStatus curStatus, PullRequestStatus? prevStatus)
    {
        // If the pull request is not recent, do not create a notification for it.
        if ((DateTime.Now - curStatus.PullRequest.UpdatedAt) > _pullRequestStaleTime)
        {
            return false;
        }

        // Compare pull request status.
        if (prevStatus is null || prevStatus.HeadSha != curStatus.HeadSha)
        {
            // No previous status for this commit, assume new PR or freshly pushed commit that was
            // successful between our syncs.
            if (curStatus.Succeeded)
            {
                return true;
            }
        }
        else
        {
            // Only post success notifications if it wasn't previously successful.
            if (curStatus.Succeeded && !prevStatus.Succeeded)
            {
                return true;
            }
        }

        return false;
    }

    // Internal method to update issues. Assumes Repository has already been populated and created.
    // DataStore transaction is assumed to be wrapped around this in the public method.
    private async Task UpdateIssuesAsync(Repository repository, Octokit.GitHubClient? client = null, RequestOptions? options = null)
    {
        options ??= RequestOptions.RequestOptionsDefault();
        client ??= await GitHubClientProvider.Instance.GetClientForLoggedInDeveloper(true);
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

    // Internal method to update releases. Assumes Repository has already been populated and created.
    // DataStore transaction is assumed to be wrapped around this in the public method.
    private async Task UpdateReleasesAsync(Repository repository, Octokit.GitHubClient? client = null, RequestOptions? options = null)
    {
        options ??= RequestOptions.RequestOptionsDefault();

        // Limit the number of fetched releases.
        options.ApiOptions.PageCount = 1;
        options.ApiOptions.PageSize = 10;

        client ??= await GitHubClientProvider.Instance.GetClientForLoggedInDeveloper(true);
        _log.Information($"Updating releases for: {repository.FullName}");

        var releasesResult = await client.Repository.Release.GetAll(repository.InternalId, options.ApiOptions);
        if (releasesResult == null)
        {
            _log.Debug($"No releases found.");
            return;
        }

        _log.Debug($"Results contain {releasesResult.Count} releases.");
        foreach (var release in releasesResult)
        {
            if (release.Draft)
            {
                continue;
            }

            _ = Release.GetOrCreateByOctokitRelease(DataStore, release, repository);
        }

        // Remove releases from this repository that were not observed recently.
        Release.DeleteLastObservedBefore(DataStore, repository.Id, DateTime.UtcNow - _lastObservedDeleteSpan);
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
        var issuesResult = await GitHubClientProvider.Instance.GetClient().Search.SearchIssues(searchIssuesRequest);
        if (issuesResult == null)
        {
            _log.Information($"No issues found.");
            return;
        }

        _log.Information($"Results contain {issuesResult.Items.Count} issues.");

        var cancellationToken = options?.CancellationToken.GetValueOrDefault() ?? default;
        var dsSearch = Search.GetOrCreate(DataStore, name, searchString, SearchType.Issues);

        foreach (var issue in issuesResult.Items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var dsIssue = Issue.GetOrCreateByOctokitIssue(DataStore, issue);
            SearchIssue.AddIssueToSearch(DataStore, dsIssue, dsSearch);
        }
    }

    private async Task UpdatePullRequestsForSearchAsync(string name, string searchString, RequestOptions? options = null)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    private async Task UpdateRepositoriesForSearchAsync(string name, string searchString, RequestOptions? options = null)
    {
        _log.Information($"Updating repositories for: {name}");
        options ??= RequestOptions.RequestOptionsDefault();
        var searchRepoRequest = new Octokit.SearchRepositoriesRequest(searchString);
        var reposResult = await GitHubClientProvider.Instance.GetClient().Search.SearchRepo(searchRepoRequest);

        if (reposResult == null)
        {
            _log.Debug($"No repositories found.");
            return;
        }

        var cancellationToken = options?.CancellationToken.GetValueOrDefault() ?? default;
        _log.Debug($"Results contain {reposResult.Items.Count} repositories.");
        var dsSearch = Search.GetOrCreate(DataStore, name, searchString, SearchType.Repositories);
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

    public async Task UpdateDataForSearchesAsync(IEnumerable<PersistentData.Search> searches, RequestOptions options)
    {
        foreach (var search in searches)
        {
            await UpdateDataForSearchAsync(search.Name, search.SearchString, (SearchType)search.TypeId, options);
        }
    }

    public Search? GetSearch(string name, string searchString, SearchType type)
    {
        ValidateDataStore();
        return Search.Get(DataStore, name, searchString, type);
    }

    // Removes unused data from the datastore.
    private void PruneObsoleteData()
    {
        CheckRun.DeleteUnreferenced(DataStore);
        CheckSuite.DeleteUnreferenced(DataStore);
        CommitCombinedStatus.DeleteUnreferenced(DataStore);
        PullRequestStatus.DeleteUnreferenced(DataStore);
        Notification.DeleteBefore(DataStore, DateTime.Now - _notificationRetentionTime);
        Search.DeleteBefore(DataStore, DateTime.Now - _searchRetentionTime);
        SearchIssue.DeleteUnreferenced(DataStore);
        SearchRepository.DeleteUnreferenced(DataStore);
        Review.DeleteUnreferenced(DataStore);
        Release.DeleteBefore(DataStore, DateTime.Now - _releaseRetentionTime);
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
