// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Pages;

namespace GitHubExtension.DataManager;

public class CacheDataManagerFacade : ICacheDataManager
{
    private readonly CacheManager _cacheManager;
    private readonly GitHubDataManager _gitHubDataManager;

    public CacheDataManagerFacade(CacheManager cacheManager, GitHubDataManager gitHubDataManager)
    {
        _cacheManager = cacheManager;
        _gitHubDataManager = gitHubDataManager;

        _cacheManager.OnUpdate += CacheManagerOnOnUpdate;
    }

    public event CacheManagerUpdateEventHandler? OnUpdate;

    private void CacheManagerOnOnUpdate(object? source, CacheManagerUpdateEventArgs e)
    {
        OnUpdate?.Invoke(source, e);
    }

    public Task<IEnumerable<IIssue>> GetIssues(ISearch search)
    {
        return Task.Run(() =>
        {
            _cacheManager.CancelUpdateInProgress();

            var res = _gitHubDataManager.GetIssuesForSearch(search.Name, search.SearchString);

            _cacheManager.RequestRefresh(UpdateType.Search, search);
            return res;
        });
    }

    public Task<IEnumerable<IPullRequest>> GetPullRequests(ISearch search)
    {
        return Task.Run(() =>
        {
            _cacheManager.CancelUpdateInProgress();

            var res = _gitHubDataManager.GetPullRequestsForSearch(search.Name, search.SearchString);

            _cacheManager.RequestRefresh(UpdateType.Search, search);
            return res;
        });
    }
}
