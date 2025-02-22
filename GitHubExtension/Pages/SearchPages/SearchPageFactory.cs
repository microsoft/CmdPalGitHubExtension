// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using GitHubExtension.DataManager;
using GitHubExtension.DataModel.Enums;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension;

public static class SearchPageFactory
{
    private static ICacheManager? cacheManager;

    // This injects the cache manager into the factory class.
    // This works to decouple tha pages from other dependencies
    // and make it easier to unit test the pages.
    public static void Initialize(ICacheManager cacheManager)
    {
        SearchPageFactory.cacheManager = cacheManager;
    }

    public static ListPage CreateForSearch(PersistentData.Search search)
    {
        return search.Type switch
        {
            SearchType.PullRequests => new PullRequestsSearchPage(search, cacheManager!),
            SearchType.Issues => new IssuesSearchPage(search, cacheManager!),
            _ => throw new NotImplementedException(),
        };
    }
}
