// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Client;
using GitHubExtension.Commands;
using GitHubExtension.DataModel.Enums;
using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Octokit;
using Serilog;

namespace GitHubExtension;

internal sealed partial class SearchPage : ListPage
{
    public PersistentData.Search CurrentSearch { get; set; }

    // Search is mandatory for this page to exist
    public SearchPage(PersistentData.Search search)
    {
        Icon = new IconInfo(GitHubIcon.IconDictionary[$"{(SearchType)search.TypeId}"]);
        Name = search.Name;
        CurrentSearch = search;
    }

    public override IListItem[] GetItems() => DoGetItems(SearchText).GetAwaiter().GetResult();

    private async Task<IEnumerable<DataModel.Issue>> LoadContentData()
    {
        return await Task.Run(() =>
        {
            var dataManager = GitHubDataManager.CreateInstance();
            var dsSearch = dataManager!.GetSearch(CurrentSearch.Name, CurrentSearch!.SearchString, (SearchType)CurrentSearch.TypeId);

            var res = new List<DataModel.Issue>();

            if (dsSearch?.Issues != null)
            {
                res.AddRange(dsSearch.Issues);
            }

            return res;
        });
    }

    private async Task<IListItem[]> DoGetItems(string query)
    {
        try
        {
            var items = await LoadContentData();

            var iconString = $"{(SearchType)CurrentSearch.TypeId}";

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

    public static string GetOwner(string repositoryUrl) => Validation.ParseOwnerFromGitHubURL(repositoryUrl);

    public static string GetRepo(string repositoryUrl) => Validation.ParseRepositoryFromGitHubURL(repositoryUrl);
}
