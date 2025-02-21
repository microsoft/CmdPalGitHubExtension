// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataModel.Enums;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension;

public class SearchPageFactory
{
    public static ListPage CreateForSearch(PersistentData.Search search)
    {
        return search.Type switch
        {
            SearchType.PullRequests => new PullRequestsSearchPage(search),
            SearchType.Issues => new IssuesSearchPage(search),
            _ => throw new NotImplementedException(),
        };
    }
}
