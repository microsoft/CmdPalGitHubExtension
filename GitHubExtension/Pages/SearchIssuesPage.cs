// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Client;
using GitHubExtension.Commands;
using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Octokit;

namespace GitHubExtension;

internal sealed partial class SearchIssuesPage : ListPage
{
    public SearchIssuesPage()
    {
        Icon = new(GitHubIcon.IconDictionary["issue"]);
        Name = "Search GitHub Issues";
        this.ShowDetails = true;
    }

    public override IListItem[] GetItems() => DoGetItems(SearchText).GetAwaiter().GetResult();

    private async Task<IListItem[]> DoGetItems(string query)
    {
        try
        {
            var issues = await GetGitHubIssuesAsync(query);

            if (issues.Count > 0)
            {
                var section = issues.Select(issue => new ListItem(new LinkCommand(issue))
                {
                    Title = issue.Title,
                    Icon = new(GitHubIcon.IconDictionary["issue"]),
                    Subtitle = $"{GetOwner(issue.Repository.HtmlUrl)}/{GetRepo(issue.Repository.HtmlUrl)}/#{issue.Number}",
                    Details = new Details()
                    {
                        Title = issue.Title,
                        Body = issue.Body,
                    },
                    MoreCommands = new CommandContextItem[]
                    {
                            new(new CopyCommand(issue.HtmlUrl, "URL")),
                            new(new CopyCommand(issue.Title, "issue title")),
                            new(new CopyCommand(issue.Number.ToString(CultureInfo.InvariantCulture), "issue number")),
                            new(new IssueMarkdownPage(issue)),
                    },
                }).ToArray();

                return section;
            }
            else
            {
                return issues.Count == 0
                    ? new ListItem[]
                    {
                            new(new NoOpCommand())
                            {
                                Title = "No issues found",
                                Icon = new(GitHubIcon.IconDictionary["issue"]),
                            },
                    }
                    :
                    [
                            new ListItem(new NoOpCommand())
                            {
                                Title = "Error fetching issues",
                                Icon = new(GitHubIcon.IconDictionary["issue"]),
                            },
                    ];
            }
        }
        catch (Exception ex)
        {
            var stackTrace = "stackTrace";

            if (ex.StackTrace != null)
            {
                stackTrace = ex.StackTrace;
            }

            return
            [
                    new ListItem(new NoOpCommand())
                    {
                        Title = "Error fetching issues",
                        Details = new Details()
                        {
                            Title = ex.Message,
                            Body = stackTrace,
                        },
                    },
            ];
        }
    }

    public string GetOwner(string repositoryUrl) => Validation.ParseOwnerFromGitHubURL(repositoryUrl);

    public string GetRepo(string repositoryUrl) => Validation.ParseRepositoryFromGitHubURL(repositoryUrl);

    private static async Task<List<Issue>> GetGitHubIssuesAsync(string query)
    {
        var devIdProvider = DeveloperIdProvider.GetInstance();
        var devIds = devIdProvider.GetLoggedInDeveloperIdsInternal();

        var client = devIds.Any() ? devIds.First().GitHubClient : GitHubClientProvider.Instance.GetClient();

        var requestOptions = new RequestOptions
        {
            UsePublicClientAsFallback = true,
        };

        if (!string.IsNullOrEmpty(query))
        {
            // TODO: if a query was provided, use that query for parameters.
        }
        else
        {
            // Default query parameters.
            // We are only interested in getting the first 10 issues. Repositories can have
            // hundreds and thousands of issues open, and the widget can only display a small
            // number of them. We also don't need all of the issues possible, just the most
            // recent which are likely of interest to the developer to watch for new issues.
            requestOptions.SearchIssuesRequest = new SearchIssuesRequest
            {
                State = ItemState.Open,
                Type = IssueTypeQualifier.Issue,
                SortField = IssueSearchSort.Created,
                Order = SortDirection.Descending,
                PerPage = 10,
                Page = 1,
            };
        }

        var searchResults = await client.Search.SearchIssues(requestOptions.SearchIssuesRequest);

        // TODO: When this value returns, there isn't all the information needed for the list items,
        // so it returns null.
        var items = searchResults.Items.Take(10).ToList();
        return new List<Issue>(items);
    }

    private static async Task<List<Issue>> GetAllIssuesAsync(GitHubClient client)
    {
        var issue_request = new IssueRequest()
        {
            Filter = IssueFilter.All,
        };

        var api_issues = await client.Issue.GetAllForCurrent(issue_request);
        var issues = new List<Issue>(api_issues);

        return issues;
    }
}
