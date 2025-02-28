// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls.Pages;
using GitHubExtension.DataManager.Enums;
using GitHubExtension.DataManager.GitHubDataManager;

namespace GitHubExtension.DataManager.CacheManager;

public class PendingRefreshState : CacheManagerState
{
    public PendingRefreshState(CacheManager cacheManager)
        : base(cacheManager)
    {
    }

    public async override Task Refresh(UpdateType updateType, ISearch? search)
    {
        await Task.Run(() =>
        {
            lock (CacheManager.GetStateLock())
            {
                if (search != null && search.SearchString == CacheManager.PendingSearch?.SearchString)
                {
                    Logger.Information("Search is the same as the pending search. Ignoring.");
                    return;
                }

                if (updateType == CacheManager.CurrentUpdateType)
                {
                    Logger.Information("Update type is the same as the current update type. Ignoring.");
                    return;
                }

                CacheManager.PendingSearch = search;
                CacheManager.CurrentUpdateType = updateType;
            }

            CacheManager.CancelUpdateInProgress();
        });
    }

    public async override void HandleDataManagerUpdate(object? source, DataManagerUpdateEventArgs e)
    {
        switch (e.Kind)
        {
            case DataManagerUpdateKind.Cancel:
                Logger.Information($"Received data manager cancellation. Refreshing for {CacheManager.PendingSearch?.Name}");
                lock (CacheManager.GetStateLock())
                {
                    CacheManager.State = CacheManager.RefreshingState;
                }

                await CacheManager.Update(TimeSpan.MinValue, CacheManager.CurrentUpdateType, CacheManager.PendingSearch);
                break;
            default:
                Logger.Information($"Received data manager update event {e.Kind}. Changing to Idle state.");

                lock (CacheManager.GetStateLock())
                {
                    CacheManager.State = CacheManager.IdleState;
                    CacheManager.PendingSearch = null;
                    CacheManager.CurrentUpdateType = UpdateType.Unknown;
                }

                break;
        }
    }
}
