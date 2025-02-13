// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;
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
        GitHubDataManager.OnUpdate += DataManagerUpdateHandler;
        PropChanged += PropChangedHandler;
    }

    ~SearchPullRequestsPage()
    {
        GitHubDataManager.OnUpdate -= DataManagerUpdateHandler;
        PropChanged -= PropChangedHandler;
    }

    private DateTime lastUpdated = DateTime.MinValue;

    public void PropChangedHandler(object? sender, IPropChangedEventArgs e)
    {
        Log.Information($"Property changed: {e.PropertyName}");
    }

    public override IListItem[] GetItems() => DoGetItems(SearchText).GetAwaiter().GetResult();

    private async void RequestContentData()
    {
        var cacheManager = CacheManager.GetInstance();
        await cacheManager.Refresh();
    }

    private async Task<List<DataModel.PullRequest>> LoadContentData()
    {
        return await Task.Run(() =>
        {
            Log.Information($"Starting loading data.");
            var repoHelper = GitHubRepositoryHelper.Instance;
            var repoCollection = repoHelper.GetUserRepositoryCollection();
            var data = new List<DataModel.PullRequest>();
            var dataManager = GitHubDataManager.CreateInstance();

            foreach (var repo in repoCollection)
            {
                try
                {
                    var repository = dataManager!.GetRepository(GetOwner(repo), GetRepo(repo));
                    Log.Information($"Got the repository {repo}.");
                    var pulls = repository?.PullRequests;
                    if (pulls != null)
                    {
                        data.AddRange(pulls);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Error loading pull requests for repository {repo}: {ex}");
                }
                finally
                {
                    Log.Information($"Finished loading pulls data for {repo}.");
                }
            }

            Log.Information($"Finishing loading data.");
            return data;
        });
    }

    public void DataManagerUpdateHandler(object? source, DataManagerUpdateEventArgs e)
    {
        if (e.Kind == DataManagerUpdateKind.Repository)
        {
            lastUpdated = DateTime.Now;
            RaiseItemsChanged(0);
        }
    }

    private async Task<IListItem[]> DoGetItems(string query)
    {
        try
        {
            Log.Information($"Pull Page GetItems command called.");

            var pullRequests = await GetGitHubPullRequestsAsync(query);
            Log.Information($"Got {pullRequests.Count} pull requests data.");

            if (pullRequests.Count > 0)
            {
                var res = pullRequests.Select(pullRequest => new ListItem(new LinkCommand(pullRequest))
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

                Log.Information($"Finished initializing pull objects.");
                return res;
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

    private async Task<List<DataModel.PullRequest>> GetGitHubPullRequestsAsync(string query)
    {
        var res = await LoadContentData();
        Log.Information($"Starting request for data.");

        RequestContentData();
        return res;
    }
}
