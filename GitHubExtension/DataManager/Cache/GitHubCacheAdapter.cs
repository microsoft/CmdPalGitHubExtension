// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.DataManager.Data;
using Serilog;

namespace GitHubExtension.DataManager.Cache;

public class GitHubCacheAdapter : IGitHubCacheDataManager
{
    private readonly IGitHubDataManager _dataManager;
    private readonly ILogger _logger;

    public GitHubCacheAdapter(IGitHubDataManager dataManager)
    {
        _dataManager = dataManager;
        _dataManager.OnUpdate += DataManagerUpdateEventHandler;
        _logger = Log.Logger.ForContext("SourceContext", nameof(GitHubCacheAdapter));
    }

    public DateTime LastUpdated
    {
        get => _dataManager.LastUpdated;

        set => _dataManager.LastUpdated = value;
    }

    public event DataManagerUpdateEventHandler? OnUpdate;

    public void DataManagerUpdateEventHandler(object? source, DataManagerUpdateEventArgs e)
    {
        OnUpdate?.Invoke(source, e);
    }

    public bool IsSearchNewOrStale(ISearch search, TimeSpan refreshCooldown)
    {
        var dsSearch = _dataManager.GetSearch(search.Name, search.SearchString);

        return dsSearch == null || DateTime.UtcNow - dsSearch.UpdatedAt > refreshCooldown;
    }

    public Task RequestAllUpdateAsync(List<ISearch> searches, RequestOptions options)
    {
        return _dataManager.RequestAllUpdateAsync(searches, options);
    }

    public Task RequestSearchUpdateAsync(ISearch search, RequestOptions options)
    {
        return _dataManager.RequestSearchUpdateAsync(search, options);
    }
}
