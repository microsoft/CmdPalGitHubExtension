// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.DataManager.Enums;
using GitHubExtension.DataModel;
using GitHubExtension.DataModel.Enums;

namespace GitHubExtension.DataManager.Data;

public partial class GitHubDataManager
{
    public event DataManagerUpdateEventHandler? OnUpdate;

    // rate limit exception gets thrown here
    private async Task PerformUpdateAsync(DataUpdateParameters parameters, Func<Task> asyncOperation)
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
            SendCancelUpdateEvent(parameters);
            return;
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _log.Error(ex, $"Error during update: {ex.Message}");
            SendErrorUpdateEvent(parameters, ex);
            return;
        }

        tx.Commit();

        SendSuccessUpdateEvent(parameters);

        _log.Information($"Update complete: {parameters}");
    }

    public async Task RequestAllUpdateAsync(List<ISearch> searches, RequestOptions options)
    {
        _log.Information("Updating all data");
        var parameters = new DataUpdateParameters
        {
            OperationName = nameof(RequestAllUpdateAsync),
            RequestOptions = options,
            UpdateType = UpdateType.All,
        };

        await PerformUpdateAsync(
            parameters,
            async () =>
            {
                await UpdateDataForSearchesAsync(searches, options);
            });

        LastUpdated = DateTime.UtcNow;
    }

    public async Task RequestSearchUpdateAsync(ISearch search, RequestOptions options)
    {
        _log.Information("Updating search data");
        var parameters = new DataUpdateParameters
        {
            OperationName = nameof(RequestSearchUpdateAsync),
            RequestOptions = options,
            UpdateType = UpdateType.Search,
            Search = search,
        };
        await PerformUpdateAsync(
            parameters,
            async () => await UpdateDataForSearchAsync(search, options));

        LastUpdated = DateTime.UtcNow;
    }

    private void SendUpdateEvent(DataManagerUpdateKind kind, UpdateType updateType, ISearch? search, Exception? ex = null)
    {
        _log.Information($"Sending Update Event: {kind}  Type: {updateType}");
        OnUpdate?.Invoke(this, new DataManagerUpdateEventArgs(kind, updateType, search, ex));
    }

    private void SendSuccessUpdateEvent(DataUpdateParameters parameters)
    {
        SendUpdateEvent(DataManagerUpdateKind.Success, parameters.UpdateType, parameters.Search, null);
    }

    private void SendCancelUpdateEvent(DataUpdateParameters parameters)
    {
        SendUpdateEvent(DataManagerUpdateKind.Cancel, parameters.UpdateType, parameters.Search, null);
    }

    private void SendErrorUpdateEvent(DataUpdateParameters parameters, Exception ex)
    {
        SendUpdateEvent(DataManagerUpdateKind.Error, parameters.UpdateType, parameters.Search, ex);
    }

    public void PurgeAllData()
    {
        DataStore.Reset();
    }
}
