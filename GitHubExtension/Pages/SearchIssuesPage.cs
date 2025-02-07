// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Client;
using GitHubExtension.Commands;
using GitHubExtension.DataManager;
using GitHubExtension.DataModel;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Serilog;

namespace GitHubExtension;

internal sealed partial class SearchIssuesPage : ListPage
{
    public SearchIssuesPage()
    {
        Icon = new IconInfo(GitHubIcon.IconDictionary["issue"]);
        Name = "Search GitHub Issues";
        this.ShowDetails = true;
    }

    private DateTime lastUpdated = DateTime.MinValue;

    public override IListItem[] GetItems() => DoGetItems(SearchText);

    private void RequestContentData()
    {
        if (DateTime.Now - lastUpdated < TimeConstants.Cooldown)
        {
            return;
        }

        var repoHelper = GitHubRepositoryHelper.Instance;
        var repoCollection = repoHelper.GetUserRepositoryCollection();
        var requestOptions = RequestOptions.RequestOptionsDefault();
        var dataManager = GitHubDataManager.CreateInstance();

        /*
        foreach (var repo in repoCollection)
        {
            _ = dataManager?.UpdateIssuesForRepositoryAsync(GetOwner(repo), GetRepo(repo), requestOptions);
        }
        */
        _ = dataManager?.UpdateIssuesForRepositoryAsync(GetOwner(repoCollection[43]), GetRepo(repoCollection[43]), requestOptions);
    }

    private List<Issue> LoadContentData()
    {
        var repoHelper = GitHubRepositoryHelper.Instance;
        var repoCollection = repoHelper.GetUserRepositoryCollection();
        var data = new List<Issue>();
        var dataManager = GitHubDataManager.CreateInstance();

        /*
        foreach (var repo in repoCollection)
        {
            var repository = dataManager!.GetRepository(GetOwner(repo), GetRepo(repo));
            var issues = repository?.Issues;
            if (issues != null)
            {
                data.AddRange(issues);
            }
        }
        */
        var repository = dataManager!.GetRepository(GetOwner(repoCollection[43]), GetRepo(repoCollection[43]));
        var issues = repository?.Issues;
        if (issues != null)
        {
            data.AddRange(issues);
        }

        return data;
    }

    public void DataManagerUpdateHandler(object? source, DataManagerUpdateEventArgs e)
    {
        if (e.Kind == DataManagerUpdateKind.Repository)
        {
            lastUpdated = DateTime.Now;
            RaiseItemsChanged(0);
        }
    }

    private IListItem[] DoGetItems(string query)
    {
        try
        {
            var issues = GetGitHubIssuesAsync(query);

            foreach (var issue in issues)
            {
                Log.Information($"{issue.Title}, {GetRepo(issue.HtmlUrl)}, {issue.Body}, {issue.Number}");
            }

            if (issues.Count > 0)
            {
                return issues.Select(issue => new ListItem(new LinkCommand(issue))
                {
                    Title = issue.Title,
                    Icon = new IconInfo(GitHubIcon.IconDictionary["issue"]),
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

    private List<Issue> GetGitHubIssuesAsync(string query)
    {
        var res = LoadContentData();
        RequestContentData();
        return res;
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
