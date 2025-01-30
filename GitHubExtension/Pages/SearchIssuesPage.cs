// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Client;
using GitHubExtension.Commands;
using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using GitHubExtension.Pages;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Octokit;
using Serilog;

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

            foreach (var issue in issues)
            {
                Log.Information($"{issue.Title}, {GetRepo(issue.HtmlUrl)}, {issue.Body}, {issue.Number}");
            }

            if (issues.Count > 0)
            {
                return issues.Select(issue => new ListItem(new LinkCommand(issue))
                {
                    Title = issue.Title,
                    Icon = new(GitHubIcon.IconDictionary["issue"]),
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

    public static string GetOwner(string repositoryUrl) => Validation.ParseOwnerFromGitHubURL(repositoryUrl);

    public static string GetRepo(string repositoryUrl) => Validation.ParseRepositoryFromGitHubURL(repositoryUrl);

    private static async Task<List<DataModel.DataObjects.Issue>> GetGitHubIssuesAsync(string query)
    {
        var devIdProvider = DeveloperIdProvider.GetInstance();
        var devIds = devIdProvider.GetLoggedInDeveloperIdsInternal();

        var client = devIds.Any() ? devIds.First().GitHubClient : GitHubClientProvider.Instance.GetClient();

        var repoHelper = GitHubRepositoryHelper.Instance;

        var repoCollection = repoHelper.GetUserRepositoryCollection();

        if (repoCollection.Count == 0)
        {
            Log.Information("No repositories found");
            return new List<DataModel.DataObjects.Issue>();
        }

        var requestOptions = RequestOptions.RequestOptionsDefault();
        requestOptions.SearchIssuesRequest.Repos = repoCollection;
        var searchResults = await client.Search.SearchIssues(requestOptions.SearchIssuesRequest);

        var issues = ConvertToDataObjectsIssue(searchResults.Items);

        return issues;
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

    public void OnRepositoryAdded(object sender, object? args)
    {
        if (args is Exception ex)
        {
            Log.Error($"Error in adding repository: {ex.Message}");
        }
        else
        {
            Log.Information("Repository added successfully!");

            RaiseItemsChanged(0);
        }
    }
}
