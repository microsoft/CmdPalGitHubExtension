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

    public void CancelUpdateInProgress()
    {
        _cacheManager.CancelUpdateInProgress();
    }

    public async Task Refresh(UpdateType updateType, ISearch search)
    {
        await _cacheManager.Refresh(updateType, search);
    }

    public Task<IEnumerable<IIssue>> GetIssues(ISearch search)
    {
        return Task.Run(() =>
        {
            var dsSearch = _gitHubDataManager.GetSearch(search.Name, search.SearchString);

            if (dsSearch is null)
            {
                return Enumerable.Empty<IIssue>();
            }

            return dsSearch.Issues;
        });
    }

    public Task<IEnumerable<IPullRequest>> GetPullRequests(ISearch search)
    {
        return Task.Run(() =>
        {
            var dsSearch = _gitHubDataManager.GetSearch(search.Name, search.SearchString);
            if (dsSearch is null)
            {
                return Enumerable.Empty<IPullRequest>();
            }

            var res = new List<IPullRequest>();

            foreach (var pr in dsSearch.PullRequests)
            {
                res.Add(new PullRequestSourceBranchDecorator(pr, _gitHubDataManager));
            }

            return res;
        });
    }
}
