// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Commands;
using GitHubExtension.Data;
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
                var section = issues.Select(issue => new ListItem(new IssueMarkdownPage(issue))
                {
                    Title = issue.Title,
                    Icon = new(GitHubIcon.IconDictionary["issue"]),
                    Details = new Details()
                    {
                        Title = issue.Title,
                        Body = issue.Body,
                    },
                    Tags = new Tag[]
                    {
                            new()
                            {
                                Text = issue.Number.ToString(CultureInfo.InvariantCulture),
                            },
                    },
                    MoreCommands = new CommandContextItem[]
                    {
                            new(new LinkCommand(issue)),
                            new(new CopyCommand(issue.HtmlUrl, "URL")),
                            new(new CopyCommand(issue.Title, "issue title")),
                            new(new CopyCommand(issue.Number.ToString(CultureInfo.InvariantCulture), "issue number")),
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

    private static async Task<List<Issue>> GetGitHubIssuesAsync(string query)
    {
        var devIdProvider = DeveloperIdProvider.GetInstance();
        var devIds = devIdProvider.GetLoggedInDeveloperIdsInternal();

        var client = devIds.Any() ? devIds.First().GitHubClient : GitHubClientProvider.Instance.GetClient();

        if (string.IsNullOrEmpty(query))
        {
            var allIssues = await GetAllIssuesAsync(client);
            return allIssues;
        }

        var request = new SearchIssuesRequest(query)
        {
            Is =
            [
                    IssueIsQualifier.Issue,
            ],
        };

        var searchResults = await client.Search.SearchIssues(request);

        return new List<Issue>(searchResults.Items);
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
