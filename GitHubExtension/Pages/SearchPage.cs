﻿// Copyright (c) Microsoft Corporation
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

internal sealed partial class SearchPage : ListPage
{
    public Search PageSearch { get; set; } = new Search();

    public SearchPage()
    {
        Icon = new IconInfo(GitHubIcon.IconDictionary["issue"]);
        Name = "Search GitHub Issues";
        this.ShowDetails = true;
    }

    public SearchPage(Search search)
    {
        Icon = new IconInfo(GitHubIcon.IconDictionary["issue"]);
        Name = search.Name;

        // TODO: put the search fields in the details
        PageSearch = search;
    }

    public override IListItem[] GetItems() => DoGetItems(SearchText).GetAwaiter().GetResult();

    private async Task<IListItem[]> DoGetItems(string query)
    {
        try
        {
            var issues = await RunSearchAsync(query);

            foreach (var issue in issues)
            {
                Log.Information($"{issue.Title}, {GetRepo(issue.HtmlUrl)}, {issue.Body}, {issue.Number}");
            }

            var iconString = PageSearch.Type.Equals("issue", StringComparison.OrdinalIgnoreCase) ? "issue" : "pullRequest";

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
                                Details = new Details()
                                {
                                    Body = "No issues found",
                                },
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

    private async Task<List<DataModel.DataObjects.Issue>> RunSearchAsync(string query)
    {
        try
        {
            var devIdProvider = DeveloperIdProvider.GetInstance();
            var devIds = devIdProvider.GetLoggedInDeveloperIdsInternal();

            var client = devIds.Any() ? devIds.First().GitHubClient : GitHubClientProvider.Instance.GetClient();

            var results = await client.Search.SearchIssues(new SearchIssuesRequest(PageSearch.SearchString));
            var issues = ConvertToDataObjectsIssue(results.Items);

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
