// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.DeveloperId;
using Octokit;

namespace GitHubExtension.PersistentData;

public class GitHubValidatorAdapter : IGitHubValidator
{
    private readonly IDeveloperIdProvider _developerIdProvider;

    public GitHubValidatorAdapter(IDeveloperIdProvider developerIdProvider)
    {
        _developerIdProvider = developerIdProvider;
    }

    public async Task ValidateSearch(ISearch search)
    {
        // TODO: Change this request depending on the search type.
        IGitHubClient? client = _developerIdProvider.GetLoggedInDeveloperIdsInternal().First().GitHubClient;
        var issuesOptions = new SearchIssuesRequest(search.SearchString)
        {
            State = ItemState.Open,
            Type = IssueTypeQualifier.Issue,
            SortField = IssueSearchSort.Updated,
            Order = SortDirection.Descending,
        };

        _ = await client.Search.SearchIssues(issuesOptions);
    }
}
