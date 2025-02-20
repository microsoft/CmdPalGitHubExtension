﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.PersistentData;

namespace GitHubExtension.DataManager.CacheManagerStates;

public class PeriodicUpdatingState : CacheManagerState
{
    public PeriodicUpdatingState(CacheManager cacheManager)
        : base(cacheManager)
    {
    }

    public async override Task Refresh(UpdateType updateType, Search? search)
    {
        await Task.Run(() =>
        {
            CacheManager.CancelUpdateInProgress();

            lock (CacheManager.GetStateLock())
            {
                CacheManager.PendingSearch = search;
                CacheManager.CurrentUpdateType = updateType;
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
            CacheManager.CurrentUpdateType = UpdateType.Unknown;
        }
    }
}
