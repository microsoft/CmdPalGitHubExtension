// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.RegularExpressions;
using Octokit;

namespace GitHubExtension.Helpers;

public static class GitHubRequestHelper
{
    public static SearchIssuesRequest GetSearchIssuesRequest(string term)
    {
        var sortField = IssueSearchSort.Created;
        var sortDirection = SortDirection.Descending;
        var result = ParseSortFromTerm(term);
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
        var result = ParseSortFromTerm(term);
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

    public static (IssueSearchSort SortField, SortDirection Direction, string UpdatedTerm)? ParseSortFromTerm(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return null;
        }

        // Regex to match sort:field-direction (e.g., sort:created-asc)
        var match = Regex.Match(term, @"sort:(\w+)-(asc|desc)", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return null;
        }

        var field = match.Groups[1].Value.ToLowerInvariant();
        var direction = match.Groups[2].Value.ToLowerInvariant();

        IssueSearchSort sortField;

        // reactions and interactions are not supported in the Octokit API
        switch (field)
        {
            case "created":
                sortField = IssueSearchSort.Created;
                break;
            case "updated":
                sortField = IssueSearchSort.Updated;
                break;
            case "comments":
                sortField = IssueSearchSort.Comments;
                break;
            default:
                return null;
        }

        var order = direction == "asc" ? SortDirection.Ascending : SortDirection.Descending;

        // Remove the sort:field-direction part from the term
        var updatedTerm = Regex.Replace(term, @"\s*sort:\w+-\w+\s*", string.Empty, RegexOptions.IgnoreCase).Trim();

        return (sortField, order, updatedTerm);
    }
}
