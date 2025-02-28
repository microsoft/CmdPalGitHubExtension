// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.DataManager.Enums;
using GitHubExtension.DataManager.GitHubDataManager;
using Serilog;

namespace GitHubExtension.DataManager.CacheManager;

public abstract class CacheManagerState
{
    protected CacheManager CacheManager { get; private set; }

    protected ILogger Logger { get; private set; }

    protected CacheManagerState(CacheManager cacheManager)
    {
        CacheManager = cacheManager;
        Logger = Log.Logger.ForContext("SourceContext", $"CacheManager/{GetType().Name}");
    }

    public abstract Task Refresh(UpdateType updateType, ISearch? search);

    public async virtual Task PeriodicUpdate()
    {
        Logger.Information("Periodic update requested. Ignoring.");
        await Task.CompletedTask;
    }

    public virtual void HandleDataManagerUpdate(object? source, DataManagerUpdateEventArgs e)
    {
        return;
    }
}
