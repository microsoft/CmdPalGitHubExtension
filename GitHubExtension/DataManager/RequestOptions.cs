// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Octokit;

namespace GitHubExtension;

public class RequestOptions
{
    public PullRequestRequest PullRequestRequest { get; set; }

    public SearchIssuesRequest SearchIssuesRequest { get; set; }

    public ApiOptions ApiOptions { get; set; }

    public CancellationToken? CancellationToken { get; set; }

    public bool Refresh { get; set; }

    public bool UsePublicClientAsFallback { get; set; }

    public RequestOptions()
    {
        PullRequestRequest = new PullRequestRequest();
        SearchIssuesRequest = new SearchIssuesRequest();
        ApiOptions = new ApiOptions();
    }

    public static RequestOptions RequestOptionsDefault()
    {
        var defaultOptions = new RequestOptions
        {
            PullRequestRequest = new PullRequestRequest
            {
                State = ItemStateFilter.Open,
                SortProperty = PullRequestSort.Updated,
                SortDirection = SortDirection.Descending,
            },
            SearchIssuesRequest = new SearchIssuesRequest
            {
                State = ItemState.Open,
                Type = IssueTypeQualifier.Issue,
                SortField = IssueSearchSort.Updated,
                Order = SortDirection.Descending,
            },
            ApiOptions = new ApiOptions
            {
                // Use default options.
            },
            UsePublicClientAsFallback = false,
        };

        return defaultOptions;
    }

    public override string ToString()
    {
        return $"{ApiOptions.PageSize} | {ApiOptions.PageCount} | {ApiOptions.StartPage}";
    }
}
