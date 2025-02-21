﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Commands;
using GitHubExtension.DataManager;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension;

internal sealed partial class IssuesSearchPage : SearchPage
{
    public IssuesSearchPage(PersistentData.Search search)
        : base(search)
    {
    }

    protected async override Task<IListItem[]> DoGetItems(string searchText)
    {
        try
        {
            Logger.Information($"Getting items for search query \"{CurrentSearch.Name}\"");
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

    private async Task<IEnumerable<DataModel.Issue>> LoadContentData()
    {
        CacheManager.GetInstance().OnUpdate += CacheManagerUpdateHandler;

        // To avoid locked database
        CacheManager.GetInstance().CancelUpdateInProgress();

        return await Task.Run(() =>
        {
            var dataManager = GitHubDataManager.CreateInstance();
            var dsSearch = dataManager!.GetSearch(CurrentSearch.Name, CurrentSearch!.SearchString);

            var res = new List<DataModel.Issue>();

            if (dsSearch?.Issues != null)
            {
                res.AddRange(dsSearch.Issues);
            }

            Logger.Information($"Found {res.Count} items matching search query \"{CurrentSearch.Name}\"");

            return res;
        });
    }

    private async Task<IEnumerable<DataModel.Issue>> GetSearchItemsAsync()
    {
        var items = await LoadContentData();
        _ = RequestContentData();
        return items;
    }
}
