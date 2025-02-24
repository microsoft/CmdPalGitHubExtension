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
    private readonly ILogger _logger;

    public SearchIssuesPage()
    {
        Icon = new IconInfo(GitHubIcon.IconDictionary["issue"]);
        Name = "Search GitHub Issues";
        this.ShowDetails = true;
        _logger = Log.ForContext("SourceContext", $"Pages/{nameof(SearchIssuesPage)}");
    }

    ~SearchIssuesPage()
    {
        CacheManager.GetInstance().OnUpdate -= CacheManagerUpdateHandler;
    }

    public override IListItem[] GetItems() => DoGetItems(SearchText).GetAwaiter().GetResult();

    private async void RequestContentData()
    {
        var cacheManager = CacheManager.GetInstance();
        await cacheManager.Refresh(UpdateType.Issues);
    }

    private async Task<List<Issue>> LoadContentData()
    {
        CacheManager.GetInstance().OnUpdate += CacheManagerUpdateHandler;

        return await Task.Run(() =>
        {
            var repoHelper = GitHubRepositoryHelper.Instance;
            var repoCollection = repoHelper.GetUserRepositoryCollection();
            var data = new List<Issue>();
            var dataManager = GitHubDataManager.CreateInstance();

            foreach (var repo in repoCollection)
            {
                try
                {
                    var repository = dataManager!.GetRepository(GetOwner(repo), GetRepo(repo));
                    var issues = repository?.Issues;
                    if (issues != null)
                    {
                        data.AddRange(issues);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error getting issues for repository {repo}: {ex.Message}");
                }
                finally
                {
                    _logger.Information($"Finished getting issues for repository {repo}.");
                }
            }

            return data;
        });
    }

    public void CacheManagerUpdateHandler(object? source, CacheManagerUpdateEventArgs e)
    {
        if (e.Kind == CacheManagerUpdateKind.Updated)
        {
            _logger.Information($"Received cache manager update event.");
            RaiseItemsChanged(0);
        }
    }

    private async Task<IListItem[]> DoGetItems(string query)
    {
        try
        {
            _logger.Information($"Issues Page GetItems command called.");
            var issues = await GetGitHubIssuesAsync(query);
            _logger.Information($"Got {issues.Count} issues data.");

            if (issues.Count > 0)
            {
                var res = issues.Select(issue => new ListItem(new LinkCommand(issue))
                {
                    Title = issue.Title,
                    Icon = new IconInfo(GitHubIcon.IconDictionary["issue"]),
                    Subtitle = $"{GetOwner(issue.HtmlUrl)}/{GetRepo(issue.HtmlUrl)}/#{issue.Number}",
                    MoreCommands = new CommandContextItem[]
                    {
                            new(new CopyCommand(issue.HtmlUrl, "URL")),
                            new(new CopyCommand(issue.Title, "issue title")),
                            new(new CopyCommand(issue.Number.ToString(CultureInfo.InvariantCulture), "issue number")),
                            new(new IssueContentPage(issue)),
                    },
                }).ToArray();

                _logger.Information($"Finished initializing issues objects.");
                return res;
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

    private async Task<List<Issue>> GetGitHubIssuesAsync(string query)
    {
        var res = await LoadContentData();

        RequestContentData();
        return res;
    }
}
