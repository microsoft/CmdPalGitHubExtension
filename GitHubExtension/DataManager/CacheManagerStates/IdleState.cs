// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.PersistentData;

namespace GitHubExtension.DataManager.CacheManagerStates;

internal sealed class IdleState : CacheManagerState
{
    public IdleState(CacheManager cacheManager)
        : base(cacheManager)
    {
    }

    public async override Task Refresh(UpdateType updateType, Search? search = null)
    {
        lock (CacheManager.GetStateLock())
        {
            CacheManager.SetState(CacheManager.RefreshingState);
            CacheManager.PendingSearch = search;
        }

        Logger.Information($"Starting refresh for {updateType}. Search: {search?.Name} - {search?.SearchString}");
        await CacheManager.Update(TimeSpan.MinValue, updateType, search);
    }

    public async override Task PeriodicUpdate()
    {
        // Only update per the update interval.
        if (DateTime.UtcNow - CacheManager.LastUpdateTime < CacheManager.UpdateInterval)
        {
            Logger.Information("Not time for periodic update.");
            return;
        }

        lock (CacheManager.GetStateLock())
        {
            CacheManager.SetState(CacheManager.PeriodicUpdatingState);
        }

        Logger.Information("Starting periodic update.");
        await CacheManager.Update(CacheManager.UpdateFrequency, UpdateType.All);
    }
}
