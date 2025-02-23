// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataManager;
using GitHubExtension.DataModel.Enums;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Microsoft.Extensions.DependencyInjection;

namespace GitHubExtension;

public class SearchPageFactory
{
    private static ICacheManager? _cacheManager;

    // This injects the cache manager into the factory class.
    // This works to decouple the pages from other dependencies
    // and make it easier to unit test the pages.
    public SearchPageFactory(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    public static ListPage CreateForSearch(PersistentData.Search search)
    {
        return search.Type switch
        {
            SearchType.PullRequests => new PullRequestsSearchPage(search, _cacheManager!),
            SearchType.Issues => new IssuesSearchPage(search, _cacheManager!),
            _ => throw new NotImplementedException(),
        };
    }
}
