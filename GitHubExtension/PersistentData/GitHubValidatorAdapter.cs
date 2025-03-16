// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Client;
using GitHubExtension.Controls;
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
        // TODO: Change this request depending on the search type.
        var client = await _gitHubClientProvider.GetClientForLoggedInDeveloper(true);
        var issuesSearch = new SearchIssuesRequest(search.SearchString);

        _ = await client.Search.SearchIssues(issuesSearch);
    }
}
