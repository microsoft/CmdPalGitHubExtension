﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Octokit;

namespace GitHubExtension.Helpers;

public static class GitHubRequestHelper
{
    public static SearchIssuesRequest GetSearchIssuesRequest(string term)
    {
        var sortField = IssueSearchSort.Created;
        var sortDirection = SortDirection.Descending;
        var result = SearchHelper.ParseSortFromTerm(term);
        if (result is (var sortFieldResult, var directionResult, var updatedTerm))
        {
            sortField = sortFieldResult;
            sortDirection = directionResult;
            term = updatedTerm;
        }

        return new SearchIssuesRequest(term)
        {
            PerPage = ExtensionConstants.PerPage,
            Page = 1,
            Type = IssueTypeQualifier.Issue,
            SortField = sortField,
            Order = sortDirection,
        };
    }

    public static SearchIssuesRequest GetSearchPullRequestsRequest(string term)
    {
        var sortField = IssueSearchSort.Created;
        var sortDirection = SortDirection.Descending;
        var result = SearchHelper.ParseSortFromTerm(term);
        if (result is (var sortFieldResult, var directionResult, var updatedTerm))
        {
            sortField = sortFieldResult;
            sortDirection = directionResult;
            term = updatedTerm;
        }

        return new SearchIssuesRequest(term)
        {
            PerPage = ExtensionConstants.PerPage,
            Page = 1,
            Type = IssueTypeQualifier.PullRequest,
            SortField = sortField,
            Order = sortDirection,
        };
    }
}
