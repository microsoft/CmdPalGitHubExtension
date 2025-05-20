// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.DataManager.Cache;
using GitHubExtension.DataModel.DataObjects;

namespace GitHubExtension.DataManager;

public class CacheDataManagerFacade : ICacheDataManager
{
    private readonly ICacheManager _cacheManager;
    private readonly IDataRequester _dataRequester;
    private readonly IDecoratorFactory _decoratorFactory;
    private readonly object _stateLock = new();

    public CacheDataManagerFacade(ICacheManager cacheManager, IDataRequester dataRequester, IDecoratorFactory decoratorFactory)
    {
        _cacheManager = cacheManager;
        _dataRequester = dataRequester;
        _decoratorFactory = decoratorFactory;

        _cacheManager.OnUpdate += CacheManagerOnOnUpdate;
    }

    private CacheManagerUpdateEventHandler? _onUpdate;

    public event CacheManagerUpdateEventHandler? OnUpdate
    {
        add
        {
            lock (_stateLock)
            {
                // Ensuring only one page is listening to the event.
                _onUpdate = value;
            }
        }

        remove
        {
            lock (_stateLock)
            {
                _onUpdate -= value;
            }
        }
    }

    private void CacheManagerOnOnUpdate(object? source, CacheManagerUpdateEventArgs e)
    {
        _onUpdate?.Invoke(source, e);
    }

    public async Task<IEnumerable<IIssue>> GetIssues(ISearch search)
    {
        if (_dataRequester.GetSearch(search.Name, search.SearchString) == null)
        {
            await _cacheManager.RequestRefresh(search);
        }

        var res = _dataRequester.GetIssuesForSearch(search.Name, search.SearchString);

        _ = _cacheManager.RequestRefresh(search);
        return res;
    }

    public async Task<IEnumerable<IPullRequest>> GetPullRequests(ISearch search)
    {
        if (_dataRequester.GetSearch(search.Name, search.SearchString) == null)
        {
            await _cacheManager.RequestRefresh(search);
        }

        var intermediateRes = _dataRequester.GetPullRequestsForSearch(search.Name, search.SearchString);
        _ = _cacheManager.RequestRefresh(search);

        var res = new List<IPullRequest>();

        foreach (var pr in intermediateRes)
        {
            res.Add(_decoratorFactory.DecorateSearchBranch(pr));
        }

        return res;
    }

    private List<IIssue> MergeIssuesAndPullRequests(IEnumerable<Issue> issues, IEnumerable<PullRequest> pullRequests)
    {
        var res = new List<IIssue>();

        int i = 0, j = 0;

        while (i < issues.Count() && j < pullRequests.Count())
        {
            if (issues.ElementAt(i).TimeUpdated < pullRequests.ElementAt(j).TimeUpdated)
            {
                res.Add(issues.ElementAt(i));
                i++;
            }
            else
            {
                res.Add(pullRequests.ElementAt(j));
                j++;
            }
        }

        res.AddRange(issues.Skip(i));
        res.AddRange(pullRequests.Skip(j));

        return res;
    }

    public async Task<IEnumerable<IIssue>> GetIssuesAndPullRequests(ISearch search)
    {
        if (_dataRequester.GetSearch(search.Name, search.SearchString) == null)
        {
            await _cacheManager.RequestRefresh(search);
        }

        var issues = _dataRequester.GetIssuesForSearch(search.Name, search.SearchString);
        var pullRequests = _dataRequester.GetPullRequestsForSearch(search.Name, search.SearchString);

        _ = _cacheManager.RequestRefresh(search);

        var res = MergeIssuesAndPullRequests(issues, pullRequests).Select(item =>
        {
            if (item is IPullRequest)
            {
                return _decoratorFactory.DecorateSearchBranch((IPullRequest)item);
            }

            return item;
        });

        return res;
    }
}
