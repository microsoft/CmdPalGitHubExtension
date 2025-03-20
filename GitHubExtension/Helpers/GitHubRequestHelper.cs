// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Octokit;

namespace GitHubExtension.Helpers;

public static class GitHubRequestHelper
{
    public static SearchIssuesRequest GetSearchIssuesRequest(string term)
    {
        return new SearchIssuesRequest(term)
        {
            PerPage = ExtensionConstants.PerPage,
            Page = 1,
            Type = IssueTypeQualifier.Issue,
        };
    }

    public static SearchIssuesRequest GetSearchPullRequestsRequest(string term)
    {
        return new SearchIssuesRequest(term)
        {
            PerPage = ExtensionConstants.PerPage,
            Page = 1,
            Type = IssueTypeQualifier.PullRequest,
        };
    }
}
