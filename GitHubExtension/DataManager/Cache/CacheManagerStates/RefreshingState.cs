// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.DataManager.Data;
using GitHubExtension.DataManager.Enums;

namespace GitHubExtension.DataManager.Cache;

public class RefreshingState : CacheManagerState
{
    public RefreshingState(CacheManager cacheManager)
        : base(cacheManager)
    {
    }

    public async override Task Refresh(ISearch search)
    {
        await Task.Run(() =>
        {
            lock (CacheManager.GetStateLock())
            {
                if (search.SearchString == CacheManager.PendingSearch?.SearchString)
                {
                    Logger.Information("Search is the same as the pending search. Ignoring.");
                    return;
                }

                CacheManager.PendingSearch = search;
                CacheManager.CurrentUpdateType = UpdateType.Search;
            }

            CacheManager.CancelUpdateInProgress();

            lock (CacheManager.GetStateLock())
            {
                CacheManager.State = CacheManager.PendingRefreshState;
            }
        });
    }

    public override void HandleDataManagerUpdate(object? source, DataManagerUpdateEventArgs e)
    {
        Logger.Information("Received data manager update event. Changing to Idle state.");
        lock (CacheManager.GetStateLock())
        {
            CacheManager.State = CacheManager.IdleState;
            CacheManager.PendingSearch = null;
            CacheManager.CurrentUpdateType = UpdateType.Unknown;
        }
    }
}
