// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Client;
using GitHubExtension.Commands;
using GitHubExtension.DataModel.DataObjects;
using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Octokit;
using Serilog;

namespace GitHubExtension;

internal sealed partial class QueryPage : ListPage
{
    public Query PageQuery { get; set; } = new Query();

    public QueryPage()
    {
        Icon = new IconInfo(GitHubIcon.IconDictionary["issue"]);
        Name = "Search GitHub Issues";
        this.ShowDetails = true;
    }

    public QueryPage(Query query)
    {
        Icon = new IconInfo(GitHubIcon.IconDictionary["issue"]);
        Name = query.Name;

        // TODO: put the query fields in the details
        PageQuery = query;
    }

    public override IListItem[] GetItems() => DoGetItems(SearchText).GetAwaiter().GetResult();

    private async Task<IListItem[]> DoGetItems(string query)
    {
        try
        {
            var issues = new List<DataModel.DataObjects.Issue>();

            if (!string.IsNullOrEmpty(PageQuery.QueryString))
            {
                issues = await RunQueryStringAsync(query);
            }
            else
            {
                issues = await RunQueryAsync(query);
            }

            foreach (var issue in issues)
            {
                Log.Information($"{issue.Title}, {GetRepo(issue.HtmlUrl)}, {issue.Body}, {issue.Number}");
            }

            var iconString = PageQuery.Type.Equals("issue", StringComparison.OrdinalIgnoreCase) ? "issue" : "pullRequest";

            if (issues.Count > 0)
            {
                return issues.Select(issue => new ListItem(new LinkCommand(issue))
                {
                    Title = issue.Title,
                    Icon = new IconInfo(GitHubIcon.IconDictionary[iconString]),
                    Subtitle = $"{GetOwner(issue.HtmlUrl)}/{GetRepo(issue.HtmlUrl)}/#{issue.Number}",
                    MoreCommands = new CommandContextItem[]
                    {
                            new(new CopyCommand(issue.HtmlUrl, "URL")),
                            new(new CopyCommand(issue.Title, "issue title")),
                            new(new CopyCommand(issue.Number.ToString(CultureInfo.InvariantCulture), "issue number")),
                            new(new IssueMarkdownPage(issue)),
                    },
                }).ToArray();
            }
            else
            {
                return issues.Count == 0
                    ? new ListItem[]
                    {
                            new(new NoOpCommand())
                            {
                                Title = "No issues found. See logs for more details.",
                                Icon = new IconInfo(GitHubIcon.IconDictionary["issue"]),
                            },
                    }
                    :
                    [
                            new ListItem(new NoOpCommand())
                            {
                                Title = "Error fetching issues",
                                Icon = new IconInfo(GitHubIcon.IconDictionary["issue"]),
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

    public static string GetOwner(string repositoryUrl) => Validation.ParseOwnerFromGitHubURL(repositoryUrl);

    public static string GetRepo(string repositoryUrl) => Validation.ParseRepositoryFromGitHubURL(repositoryUrl);

    private async Task<List<DataModel.DataObjects.Issue>> RunQueryStringAsync(string query)
    {
        try
        {
            var devIdProvider = DeveloperIdProvider.GetInstance();
            var devIds = devIdProvider.GetLoggedInDeveloperIdsInternal();

            var client = devIds.Any() ? devIds.First().GitHubClient : GitHubClientProvider.Instance.GetClient();

            var results = await client.Search.SearchIssues(new SearchIssuesRequest(PageQuery.QueryString));
            var issues = ConvertToDataObjectsIssue(results.Items);

            return issues;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error running query");
            throw;
        }
    }

    private async Task<List<DataModel.DataObjects.Issue>> RunQueryAsync(string query)
    {
        try
        {
            var devIdProvider = DeveloperIdProvider.GetInstance();
            var devIds = devIdProvider.GetLoggedInDeveloperIdsInternal();

            var client = devIds.Any() ? devIds.First().GitHubClient : GitHubClientProvider.Instance.GetClient();

            var options = RequestOptions.RequestOptionsDefault();

            // TODO: Implement type filtering (right now, this code searches both issues and pull requests)

            // set options for search based on the query values - TODO: Implement Owner

            // This assumes the user properly typed the repo as "owner/repo"
            if (!string.IsNullOrEmpty(PageQuery.Repository))
            {
                options.SearchIssuesRequest.Repos = new RepositoryCollection { $"{PageQuery.Repository}" };
            }

            options.SearchIssuesRequest.Assignee = string.IsNullOrEmpty(PageQuery.Assignee) ? null : PageQuery.Assignee;
            options.SearchIssuesRequest.Author = string.IsNullOrEmpty(PageQuery.Author) ? null : PageQuery.Author;

            if (!string.IsNullOrEmpty(PageQuery.Type))
            {
                if (string.Equals(PageQuery.Type, "pull request", StringComparison.OrdinalIgnoreCase))
                {
                    options.SearchIssuesRequest.Type = IssueTypeQualifier.PullRequest;
                }
                else
                {
                    options.SearchIssuesRequest.Type = IssueTypeQualifier.Issue;
                }
            }

            // TODO: Support multiple labels
            if (!string.IsNullOrEmpty(PageQuery.Labels))
            {
                options.SearchIssuesRequest.Labels = new List<string> { PageQuery.Labels };
            }

            options.SearchIssuesRequest.Mentions = string.IsNullOrEmpty(PageQuery.MentionedUsers) ? null : PageQuery.MentionedUsers;

            if (string.Equals(PageQuery.State, "open/closed", StringComparison.OrdinalIgnoreCase))
            {
                // do nothing, Octokit will search for open by default?? TODO: Investigate
            }
            else
            {
                options.SearchIssuesRequest.State = string.Equals(PageQuery.State, "open", StringComparison.OrdinalIgnoreCase) ? ItemState.Open : ItemState.Closed;
            }

            // get the search results (and how will we know what we're searching for?)
            var searchResults = await client.Search.SearchIssues(options.SearchIssuesRequest);

            // convert the search results to a DataObject that can come back
            var issues = ConvertToDataObjectsIssue(searchResults.Items);
            return issues;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error running query");
            throw;
        }
    }

    private static List<DataModel.DataObjects.Issue> ConvertToDataObjectsIssue(IReadOnlyList<Octokit.Issue> octokitIssueList)
    {
        var dataModelIssues = new List<DataModel.DataObjects.Issue>();
        foreach (var octokitIssue in octokitIssueList)
        {
            var issue = DataModel.DataObjects.Issue.CreateFromOctokitIssue(octokitIssue);
            dataModelIssues.Add(issue);
        }

        return dataModelIssues;
    }
}
