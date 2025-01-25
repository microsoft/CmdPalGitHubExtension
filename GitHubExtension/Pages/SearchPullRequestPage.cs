// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Client;
using GitHubExtension.Commands;
using GitHubExtension.DataModel.DataObjects;
using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using GitHubExtension.Pages;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Octokit;
using Octokit.Internal;
using Serilog;

namespace GitHubExtension;

internal sealed partial class SearchPullRequestsPage : ListPage
{
    public SearchPullRequestsPage()
    {
        Icon = new(GitHubIcon.IconDictionary["pullRequest"]);
        Name = "Search GitHub Pull Requests";
        this.ShowDetails = true;
    }

    public override IListItem[] GetItems() => DoGetItems(SearchText).GetAwaiter().GetResult();

    private async Task<IListItem[]> DoGetItems(string query)
    {
        try
        {
            var pullRequests = await GetGitHubPullRequestsAsync(query);

            foreach (var pullRequest in pullRequests)
            {
                Log.Information($"{pullRequest.Title}, {GetRepo(pullRequest.HtmlUrl)}, {pullRequest.Body}, {pullRequest.Number}");
            }

            if (pullRequests.Count > 0)
            {
                return pullRequests.Select(pullRequest => new ListItem(new LinkCommand(pullRequest))
                {
                    Title = pullRequest.Title,
                    Icon = new(GitHubIcon.IconDictionary["pullRequest"]),
                    Subtitle = $"{GetOwner(pullRequest.HtmlUrl)}/{GetRepo(pullRequest.HtmlUrl)}/#{pullRequest.Number}",
                    Details = new Details()
                    {
                        Title = pullRequest.Title,
                        Body = pullRequest.Body,
                    },
                    MoreCommands = new CommandContextItem[]
                    {
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
                                Icon = new(GitHubIcon.IconDictionary["pullRequest"]),
                            },
                    }
                    :
                    [
                            new ListItem(new NoOpCommand())
                            {
                                Title = "Error fetching pull requests",
                                Icon = new(GitHubIcon.IconDictionary["pullRequest"]),
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

    private static async Task<List<DataModel.DataObjects.PullRequest>> GetGitHubPullRequestsAsync(string query)
    {
        var devIdProvider = DeveloperIdProvider.GetInstance();
        var devIds = devIdProvider.GetLoggedInDeveloperIdsInternal();

        var client = devIds.Any() ? devIds.First().GitHubClient : GitHubClientProvider.Instance.GetClient();

        var repoHelper = new GitHubRepositoryHelper(client);

        var repos = repoHelper.GetUserRepositoriesAsync().GetAwaiter().GetResult();

        var defaultPullRequest = new PullRequestRequest
        {
            State = ItemStateFilter.Open,
            SortProperty = PullRequestSort.Created,
            SortDirection = SortDirection.Descending,
        };

        var pullRequests = new List<Octokit.PullRequest>();
        foreach (var repo in repos)
        {
            var repoPRs = await client.PullRequest.GetAllForRepository(repo.Owner.Login, repo.Name, defaultPullRequest);
            pullRequests.AddRange(repoPRs);
        }

        var pullRequestDataObjects = ConvertToDataObjectsPullRequest(pullRequests);

        return pullRequestDataObjects;
    }

    private static List<DataModel.DataObjects.PullRequest> ConvertToDataObjectsPullRequest(IReadOnlyList<Octokit.PullRequest> octokitPullRequestList)
    {
        var dataModelPullRequests = new List<DataModel.DataObjects.PullRequest>();
        foreach (var octokitPullRequest in octokitPullRequestList)
        {
            var pullRequest = DataModel.DataObjects.PullRequest.CreateFromOctokitPullRequest(octokitPullRequest);
            dataModelPullRequests.Add(pullRequest);
        }

        return dataModelPullRequests;
    }
}
