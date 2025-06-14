﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.DataManager.Cache;
using GitHubExtension.DataModel.DataObjects;
using GitHubExtension.Helpers;

namespace GitHubExtension.DataManager;

public sealed class CacheDataManagerFacade : ICacheDataManager, IDisposable
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

    private readonly WeakEventSource<CacheManagerUpdateEventArgs> _onUpdateWeakSource = new();

    public event EventHandler<CacheManagerUpdateEventArgs>? OnUpdateWeak
    {
        add => _onUpdateWeakSource.Subscribe(value);
        remove => _onUpdateWeakSource.Unsubscribe(value);
    }

    private void CacheManagerOnOnUpdate(object? source, CacheManagerUpdateEventArgs e)
    {
        _onUpdateWeakSource.Raise(source, e);
    }

    private async Task DownloadSearch(ISearch search)
    {
        if (_dataRequester.GetSearch(search.Name, search.SearchString) == null)
        {
            var tcs = new TaskCompletionSource();
            CacheManagerUpdateEventHandler? handler = null;
            handler = (s, e) =>
            {
                if (e.Kind == CacheManagerUpdateKind.Updated && (e.Search == null || e.Search == search))
                {
                    tcs.TrySetResult();
                    _cacheManager.OnUpdate -= handler;
                }
            };

            _cacheManager.OnUpdate += handler;
            _ = _cacheManager.RequestRefresh(search);

            await tcs.Task;
        }
    }

    public async Task<IEnumerable<IIssue>> GetIssues(ISearch search)
    {
        await DownloadSearch(search);

        var res = _dataRequester.GetIssuesForSearch(search.Name, search.SearchString);

        _ = _cacheManager.RequestRefresh(search);
        return res;
    }

    public async Task<IEnumerable<IPullRequest>> GetPullRequests(ISearch search)
    {
        await DownloadSearch(search);

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
        await DownloadSearch(search);

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

    // Disposing area
    private bool _disposed;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _cacheManager.OnUpdate -= CacheManagerOnOnUpdate;
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
