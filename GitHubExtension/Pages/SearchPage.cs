// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Client;
using GitHubExtension.Commands;
using GitHubExtension.DataManager;
using GitHubExtension.DataModel.Enums;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Serilog;

namespace GitHubExtension;

internal sealed partial class SearchPage : ListPage
{
    private readonly ILogger _logger;

    public PersistentData.Search CurrentSearch { get; set; }

    private bool _requestedData;

    // Search is mandatory for this page to exist
    public SearchPage(PersistentData.Search search)
    {
        Icon = new IconInfo(GitHubIcon.IconDictionary[$"{search.Type}"]);
        Name = search.Name;
        CurrentSearch = search;
        _logger = Log.ForContext("SourceContext", $"Pages/{nameof(SearchPage)}");
    }

    public override IListItem[] GetItems() => DoGetItems(SearchText).GetAwaiter().GetResult();

    public async void RequestContentData()
    {
        var cacheManager = CacheManager.GetInstance();
        await cacheManager.Refresh(UpdateType.Search, CurrentSearch);
    }

    private async Task<IEnumerable<DataModel.Issue>> LoadContentData()
    {
        CacheManager.GetInstance().OnUpdate += CacheManagerUpdateHandler;

        return await Task.Run(() =>
        {
            // FIXME: The DataManager doesn't have the saved searches, so dsSearch is always null
            var dataManager = GitHubDataManager.CreateInstance();
            var dsSearch = dataManager!.GetSearch(CurrentSearch.Name, CurrentSearch!.SearchString);

            var res = new List<DataModel.Issue>();

            if (dsSearch?.Issues != null)
            {
                res.AddRange(dsSearch.Issues);
            }

            _logger.Information($"Found {res.Count} items matching search query \"{CurrentSearch.Name}\"");

            return res;
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
            _logger.Information($"Getting items for search query \"{CurrentSearch.Name}\"");
            var items = await GetSearchItemsAsync();

            var iconString = $"{CurrentSearch.Type}";

            if (items.Any())
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
                return !items.Any()
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

    private async Task<IEnumerable<DataModel.Issue>> GetSearchItemsAsync()
    {
        var items = await LoadContentData();

        if (!_requestedData)
        {
            RequestContentData();
            _requestedData = true;
        }

        return items;
    }

    public static string GetOwner(string repositoryUrl) => Validation.ParseOwnerFromGitHubURL(repositoryUrl);

    public static string GetRepo(string repositoryUrl) => Validation.ParseRepositoryFromGitHubURL(repositoryUrl);
}
