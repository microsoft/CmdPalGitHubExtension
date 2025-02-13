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

internal sealed partial class SearchPage : ListPage
{
    public Search CurrentSearch { get; set; } = new Search();

    public SearchPage()
    : this(new Search())
    {
    }

    public SearchPage(Search search)
    {
        Icon = new IconInfo(GitHubIcon.IconDictionary[search.Type]);
        Name = search.Name;
        CurrentSearch = search;
    }

    public override IListItem[] GetItems() => DoGetItems(SearchText).GetAwaiter().GetResult();

    private async Task<IListItem[]> DoGetItems(string query)
    {
        try
        {
            var items = await RunSearchAsync(query);

            var iconString = CurrentSearch.Type;

            if (items.Count > 0)
            {
                return items.Select(item => new ListItem(new LinkCommand(item))
                {
                    Title = item.Title,
                    Icon = new IconInfo(GitHubIcon.IconDictionary[iconString]),
                    Subtitle = $"{GetOwner(item.HtmlUrl)}/{GetRepo(item.HtmlUrl)}/#{item.Number}",
                    MoreCommands = new CommandContextItem[]
                    {
                            new(new CopyCommand(item.HtmlUrl, "URL")),
                            new(new CopyCommand(item.Title, "item title")),
                            new(new CopyCommand(item.Number.ToString(CultureInfo.InvariantCulture), "item number")),
                            new(new IssueMarkdownPage(item)),
                    },
                }).ToArray();
            }
            else
            {
                return items.Count == 0
                    ? new ListItem[]
                    {
                            new(new NoOpCommand())
                            {
                                Title = "No items found. See logs for more details.",
                                Icon = new IconInfo(GitHubIcon.IconDictionary[iconString]),
                            },
                    }
                    :
                    [
                            new ListItem(new NoOpCommand())
                            {
                                Title = "Error fetching items",
                                Details = new Details()
                                {
                                    Body = "No items found",
                                },
                                Icon = new IconInfo(GitHubIcon.IconDictionary[iconString]),
                            },
                    ];
            }
        }
        catch (Exception ex)
        {
            return
            [
                    new ListItem(new NoOpCommand())
                    {
                        Title = "Error fetching items",
                        Details = new Details()
                        {
                            Title = ex.Message,
                            Body = string.IsNullOrEmpty(ex.StackTrace) ? "There is no stack trace for the error." : ex.StackTrace,
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

            var results = await client.Search.SearchIssues(new SearchIssuesRequest(CurrentSearch.SearchString));
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
            var item = DataModel.DataObjects.Issue.CreateFromOctokitIssue(octokitIssue);
            dataModelIssues.Add(item);
        }

        return dataModelIssues;
    }
}
