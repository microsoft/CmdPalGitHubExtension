// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.DataManager.Cache;
using GitHubExtension.DataManager.Data;
using GitHubExtension.DataManager.Enums;
using GitHubExtension.DataModel.DataObjects;

namespace GitHubExtension.DataManager;

public class CacheDataManagerFacade : ICacheDataManager
{
    private readonly ICacheManager _cacheManager;
    private readonly IDataRequester _dataRequester;

    public CacheDataManagerFacade(ICacheManager cacheManager, IDataRequester dataRequester)
    {
        _cacheManager = cacheManager;
        _dataRequester = dataRequester;

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

            var res = _dataRequester.GetIssuesForSearch(search.Name, search.SearchString);

            _cacheManager.RequestRefresh(UpdateType.Search, search);
            return res as IEnumerable<IIssue>;
        });
    }

    public Task<IEnumerable<IPullRequest>> GetPullRequests(ISearch search)
    {
        return Task.Run(() =>
        {
            _cacheManager.CancelUpdateInProgress();

            var intermediateRes = _dataRequester.GetPullRequestsForSearch(search.Name, search.SearchString);
            _cacheManager.RequestRefresh(UpdateType.Search, search);

            var res = new List<IPullRequest>();

            foreach (var pr in intermediateRes)
            {
                res.Add(new PullRequestSourceBranchDecorator(pr, (IPullRequestUpdater)_dataRequester));
            }

            return res as IEnumerable<IPullRequest>;
        });
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

    public Task<IEnumerable<IIssue>> GetIssuesAndPullRequests(ISearch search)
    {
        return Task.Run(() =>
        {
            _cacheManager.CancelUpdateInProgress();

            var issues = _dataRequester.GetIssuesForSearch(search.Name, search.SearchString);
            var pullRequests = _dataRequester.GetPullRequestsForSearch(search.Name, search.SearchString);

            _cacheManager.RequestRefresh(UpdateType.Search, search);

            var res = MergeIssuesAndPullRequests(issues, pullRequests).Select(item =>
            {
                if (item is IPullRequest)
                {
                    return new PullRequestSourceBranchDecorator((IPullRequest)item, (IPullRequestUpdater)_dataRequester);
                }

                return item;
            });

            return res as IEnumerable<IIssue>;
        });
    }
}
