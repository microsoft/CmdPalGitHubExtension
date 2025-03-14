// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.DataManager.Enums;

namespace GitHubExtension.DataManager.Cache;

public class IdleState : CacheManagerState
{
    public IdleState(CacheManager cacheManager)
        : base(cacheManager)
    {
    }

    public async override Task Refresh(ISearch search)
    {
        lock (CacheManager.GetStateLock())
        {
            CacheManager.State = CacheManager.RefreshingState;
            CacheManager.PendingSearch = search;
            CacheManager.CurrentUpdateType = UpdateType.Search;
        }

        Logger.Information($"Starting refresh for Search: {search.Name} - {search.SearchString}");
        await CacheManager.Update(TimeSpan.MinValue, UpdateType.Search, search);
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
            CacheManager.State = CacheManager.PeriodicUpdatingState;
        }

        Logger.Information("Starting periodic update.");
        await CacheManager.Update(CacheManager.UpdateFrequency, UpdateType.All);
    }
}
