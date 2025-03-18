// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Client;
using GitHubExtension.Controls;
using GitHubExtension.DataModel.Enums;
using GitHubExtension.Helpers;
using Octokit;

namespace GitHubExtension.PersistentData;

public class GitHubValidatorAdapter : IGitHubValidator
{
    private readonly GitHubClientProvider _gitHubClientProvider;

    public GitHubValidatorAdapter(GitHubClientProvider gitHubClientProvider)
    {
        _gitHubClientProvider = gitHubClientProvider;
    }

    public async Task ValidateSearch(ISearch search)
    {
        var client = await _gitHubClientProvider.GetClientForLoggedInDeveloper(true);

        switch (search.Type)
        {
            case SearchType.IssuesAndPullRequests:
            case SearchType.Issues:
                var searchIssuesRequest = GitHubRequestHelper.GetSearchPullRequestsRequest(search.SearchString);
                _ = await client.Search.SearchIssues(searchIssuesRequest);
                break;
            case SearchType.PullRequests:
                var searchPullRequestsRequest = GitHubRequestHelper.GetSearchPullRequestsRequest(search.SearchString);
                _ = await client.Search.SearchIssues(searchPullRequestsRequest);
                break;
            case SearchType.Repositories:
                throw new NotImplementedException();
            default:
                throw new ArgumentException("Invalid search type");
        }
    }
}
