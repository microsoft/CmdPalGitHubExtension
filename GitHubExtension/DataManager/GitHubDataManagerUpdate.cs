// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataManager;
using GitHubExtension.DataModel;
using GitHubExtension.DataModel.Enums;

namespace GitHubExtension;

public partial class GitHubDataManager
{
    // This is how frequently the DataStore update occurs.
    private static readonly TimeSpan _updateInterval = TimeSpan.FromMinutes(5);
    private static DateTime _lastUpdateTime = DateTime.MinValue;

    private async Task PerformUpdateAsync(DataStoreOperationParameters parameters, Func<Task> asyncOperation)
    {
        using var tx = DataStore.Connection!.BeginTransaction();

        try
        {
            await asyncOperation();
            PruneObsoleteData();
            SetLastUpdatedInMetaData();
        }
        catch (HttpRequestException)
        {
            tx.Rollback();
            _log.Error($"HttpRequestException during update: {parameters}");
            throw;
        }
        catch (Exception ex) when (IsCancelException(ex))
        {
            tx.Rollback();
            _log.Information($"Update cancelled: {parameters}");
            SendCancelUpdateEvent(parameters.UpdateType);
            return;
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _log.Error(ex, $"Error during update: {ex.Message}");
            SendErrorUpdateEvent(parameters.UpdateType, ex);
            return;
        }

        tx.Commit();

        if (parameters.UpdateType == UpdateType.Search)
        {
            // Maybe we don't need this. Only the open page can raise
            // the ItemsChanged event. So if a search is updated in the background,
            // the open page is the only one that could have requested it.
            // So having the name might not matter at all.
            SendSearchSuccessUpdateEvent(parameters.SearchName!, parameters.SearchType);
        }
        else
        {
            SendSuccessUpdateEvent(parameters.UpdateType);
        }

        _log.Information($"Update complete: {parameters}");
    }

    public async Task RequestAllUpdateAsync(Octokit.RepositoryCollection repoCollection, List<PersistentData.Search> searches, RequestOptions options)
    {
        _log.Information("Updating all data");
        var parameters = new DataStoreOperationParameters
        {
            OperationName = nameof(RequestAllUpdateAsync),
            RequestOptions = options,
            UpdateType = UpdateType.All,
        };

        await PerformUpdateAsync(
            parameters,
            async () =>
            {
                await UpdateAllDataForRepositoriesAsync(repoCollection, options);
                await UpdateDataForSearchesAsync(searches, options);
            });

        _lastUpdateTime = DateTime.UtcNow;
    }

    public async Task RequestIssuesUpdateAsync(Octokit.RepositoryCollection repoCollection, RequestOptions options)
    {
        _log.Information("Updating issues data");

        var parameters = new DataStoreOperationParameters
        {
            OperationName = nameof(RequestIssuesUpdateAsync),
            RequestOptions = options,
            UpdateType = UpdateType.Issues,
        };

        await PerformUpdateAsync(
            parameters,
            async () => await UpdateIssuesForRepositoriesAsync(repoCollection, options));

        _lastUpdateTime = DateTime.UtcNow;
    }

    public async Task RequestPullRequestsUpdateAsync(Octokit.RepositoryCollection repoCollection, RequestOptions options)
    {
        _log.Information("Updating pull requests data");
        var parameters = new DataStoreOperationParameters
        {
            OperationName = nameof(RequestPullRequestsUpdateAsync),
            RequestOptions = options,
            UpdateType = UpdateType.PullRequests,
        };

        await PerformUpdateAsync(
            parameters,
            async () => await UpdatePullRequestsForRepositoriesAsync(repoCollection, options));

        _lastUpdateTime = DateTime.UtcNow;
    }

    public async Task RequestSearchUpdateAsync(string name, string searchString, SearchType type, RequestOptions options)
    {
        _log.Information("Updating search data");
        var parameters = new DataStoreOperationParameters
        {
            OperationName = nameof(RequestSearchUpdateAsync),
            RequestOptions = options,
            UpdateType = UpdateType.Search,
            SearchName = name,
            SearchType = type,
        };
        await PerformUpdateAsync(
            parameters,
            async () => await UpdateDataForSearchAsync(name, searchString, type, options));

        _lastUpdateTime = DateTime.UtcNow;
    }

    private void SendDeveloperUpdateEvent(IGitHubDataManager? source)
    {
        SendUpdateEvent(DataManagerUpdateKind.Success, UpdateType.Developer, null, null);
    }

    private void SendRepositoryUpdateEvent(string fullName, string[] context)
    {
        SendUpdateEvent(DataManagerUpdateKind.Success, UpdateType.Repository, fullName, context);
    }

    private void SendUpdateEvent(DataManagerUpdateKind kind, UpdateType updateType, string? info = null, string[]? context = null, Exception? ex = null)
    {
        if (OnUpdate != null)
        {
            info ??= string.Empty;
            context ??= Array.Empty<string>();
            _log.Information($"Sending Update Event: {kind}  Type: {updateType} Info: {info}  Context: {string.Join(",", context)}");
            OnUpdate.Invoke(this, new DataManagerUpdateEventArgs(kind, updateType, info, context, ex));
        }
    }

    private void SendSuccessUpdateEvent(UpdateType updateType)
    {
        SendUpdateEvent(DataManagerUpdateKind.Success, updateType, null, null);
    }

    private void SendSearchSuccessUpdateEvent(string searchName, SearchType searchType)
    {
        SendUpdateEvent(DataManagerUpdateKind.Success, UpdateType.Search, $"{searchName}:{searchType}", null);
    }

    private void SendCancelUpdateEvent(UpdateType updateType)
    {
        SendUpdateEvent(DataManagerUpdateKind.Cancel, updateType, null, null);
    }

    private void SendErrorUpdateEvent(UpdateType updateType, Exception ex)
    {
        SendUpdateEvent(DataManagerUpdateKind.Error, updateType, null, null, ex);
    }
}
