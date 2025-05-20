// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.DataManager.Data;
using GitHubExtension.DataManager.Enums;

namespace GitHubExtension.DataManager.Cache;

public class PendingRefreshState : CacheManagerState
{
    public PendingRefreshState(CacheManager cacheManager)
        : base(cacheManager)
    {
    }

    public async override Task Refresh(ISearch search)
    {
        await Task.Run(() =>
        {
            if (search.SearchString == CacheManager.PendingSearch?.SearchString)
            {
                Logger.Information("Search is the same as the pending search. Ignoring.");
                return;
            }

            CacheManager.PendingSearch = search;
            CacheManager.CurrentUpdateType = UpdateType.Search;

            CacheManager.CancelUpdateInProgress();
        });
    }

    public async override void HandleDataManagerUpdate(object? source, DataManagerUpdateEventArgs e)
    {
        switch (e.Kind)
        {
            case DataManagerUpdateKind.Cancel:
                Logger.Information($"Received data manager cancellation. Refreshing for {CacheManager.PendingSearch?.Name}");
                CacheManager.State = CacheManager.RefreshingState;

                await CacheManager.Update(CacheManager.CurrentUpdateType, CacheManager.PendingSearch);
                break;
            default:
                Logger.Information($"Received data manager update event {e.Kind}. Changing to Idle state.");

                CacheManager.State = CacheManager.IdleState;
                CacheManager.PendingSearch = null;
                CacheManager.CurrentUpdateType = UpdateType.Unknown;

                break;
        }
    }
}
