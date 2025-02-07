// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Client;
using GitHubExtension.Commands;
using GitHubExtension.DataManager;
using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Serilog;

namespace GitHubExtension;

internal sealed partial class SearchPullRequestsPage : ListPage
{
    public SearchPullRequestsPage()
    {
        Icon = new IconInfo(GitHubIcon.IconDictionary["pullRequest"]);
        Name = "Search GitHub Pull Requests";
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
            _ = dataManager?.UpdatePullRequestsForRepositoryAsync(GetOwner(repo), GetRepo(repo), requestOptions);
        }
        */
        _ = dataManager?.UpdatePullRequestsForRepositoryAsync(GetOwner(repoCollection[43]), GetRepo(repoCollection[43]), requestOptions);
    }

    private List<DataModel.PullRequest> LoadContentData()
    {
        var repoHelper = GitHubRepositoryHelper.Instance;
        var repoCollection = repoHelper.GetUserRepositoryCollection();
        var data = new List<DataModel.PullRequest>();
        var dataManager = GitHubDataManager.CreateInstance();

        /*
        foreach (var repo in repoCollection)
        {
            var repository = dataManager!.GetRepository(GetOwner(repo), GetRepo(repo));
            var pulls = repository?.PullRequests;
            if (pulls != null)
            {
                data.AddRange(pulls);
            }
        }
        */

        var repository = dataManager!.GetRepository(GetOwner(repoCollection[43]), GetRepo(repoCollection[43]));
        var pulls = repository?.PullRequests;
        if (pulls != null)
        {
            data.AddRange(pulls);
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
            var pullRequests = GetGitHubPullRequestsAsync(query);

            foreach (var pullRequest in pullRequests)
            {
                Log.Information($"{pullRequest.Title}, {GetRepo(pullRequest.HtmlUrl)}, {pullRequest.Body}, {pullRequest.Number}");
            }

            if (pullRequests.Count > 0)
            {
                return pullRequests.Select(pullRequest => new ListItem(new LinkCommand(pullRequest))
                {
                    Title = pullRequest.Title,
                    Icon = new IconInfo(GitHubIcon.IconDictionary["pullRequest"]),
                    Subtitle = $"{GetOwner(pullRequest.HtmlUrl)}/{GetRepo(pullRequest.HtmlUrl)}/#{pullRequest.Number}",
                    MoreCommands = new CommandContextItem[]
                    {
                            new(new CopyCommand($"git checkout {pullRequest.SourceBranch}", "checkout command")),
                            new(new CopyCommand(pullRequest.SourceBranch, "source branch")),
                            new(new CopyCommand(pullRequest.HtmlUrl, "URL")),
                            new(new CopyCommand(pullRequest.Title, "pull request title")),
                            new(new CopyCommand(pullRequest.Number.ToString(CultureInfo.InvariantCulture), "pull request number")),
                            new(new PullRequestMarkdownPage(pullRequest)),
                    },
                }).ToArray();
            }
            else
            {
                return pullRequests.Count == 0
                    ? new ListItem[]
                    {
                            new(new NoOpCommand())
                            {
                                Title = "No pull requests found",
                                Icon = new IconInfo(GitHubIcon.IconDictionary["pullRequest"]),
                            },
                    }
                    :
                    [
                            new ListItem(new NoOpCommand())
                            {
                                Title = "Error fetching pull requests",
                                Icon = new IconInfo(GitHubIcon.IconDictionary["pullRequest"]),
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
                        Title = "Error fetching pull requests",
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

    private List<DataModel.PullRequest> GetGitHubPullRequestsAsync(string query)
    {
        var res = LoadContentData();
        RequestContentData();
        return res;
    }
}
