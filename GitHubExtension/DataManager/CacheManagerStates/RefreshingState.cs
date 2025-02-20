﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.PersistentData;

namespace GitHubExtension.DataManager.CacheManagerStates;

public class RefreshingState : CacheManagerState
{
    public RefreshingState(CacheManager cacheManager)
        : base(cacheManager)
    {
    }

    public async override Task Refresh(UpdateType updateType, Search? search)
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

            lock (CacheManager.GetStateLock())
            {
                CacheManager.SetState(CacheManager.PendingRefreshState);
            }
        });
    }

    public override void HandleDataManagerUpdate(object? source, DataManagerUpdateEventArgs e)
    {
        Logger.Information("Received data manager update event. Changing to Idle state.");
        lock (CacheManager.GetStateLock())
        {
            CacheManager.SetState(CacheManager.IdleState);
            CacheManager.PendingSearch = null;
        }
    }
}
