// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls.Commands;
using GitHubExtension.DataManager;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Serilog;

namespace GitHubExtension.Controls.Pages;

// Lists GitHub discussions matching a search query. The user's typed text is
// combined with an optional base query (for example "author:@me" for a "My
// Discussions" command). When there is nothing to search, a prompt is shown.
public sealed partial class DiscussionsPage : DynamicListPage
{
    private readonly IDiscussionsDataManager _dataManager;
    private readonly IResources _resources;
    private readonly string _baseQuery;
    private readonly ILogger _log;

    public DiscussionsPage(IDiscussionsDataManager dataManager, IResources resources, string name, string baseQuery = "")
    {
        _dataManager = dataManager;
        _resources = resources;
        _baseQuery = baseQuery;
        _log = Log.ForContext("SourceContext", $"Pages/{nameof(DiscussionsPage)}");

        Name = name;
        Icon = GitHubIcon.IconDictionary["Discussions"];
        PlaceholderText = resources.GetResource("Pages_Discussions_Placeholder");
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        RaiseItemsChanged(0);
    }

    public override IListItem[] GetItems() => DoGetItems(SearchText).GetAwaiter().GetResult();

    private string BuildQuery(string searchText)
    {
        var parts = new[] { _baseQuery, (searchText ?? string.Empty).Trim() }
            .Where(part => !string.IsNullOrWhiteSpace(part));
        return string.Join(" ", parts).Trim();
    }

    private async Task<IListItem[]> DoGetItems(string query)
    {
        var effectiveQuery = BuildQuery(query);

        if (string.IsNullOrEmpty(effectiveQuery))
        {
            return new IListItem[]
            {
                new ListItem(new NoOpCommand())
                {
                    Title = _resources.GetResource("Pages_Discussions_Prompt"),
                    Icon = GitHubIcon.IconDictionary["Discussions"],
                },
            };
        }

        try
        {
            var discussions = (await _dataManager.SearchDiscussionsAsync(effectiveQuery)).ToList();

            if (discussions.Count == 0)
            {
                return new IListItem[]
                {
                    new ListItem(new NoOpCommand())
                    {
                        Title = _resources.GetResource("Pages_Discussions_None"),
                        Icon = GitHubIcon.IconDictionary["Discussions"],
                    },
                };
            }

            return discussions.Select(GetListItem).ToArray();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to load discussions.");
            return new IListItem[]
            {
                new ListItem(new NoOpCommand())
                {
                    Title = _resources.GetResource("Pages_Error_Title"),
                    Details = new Details()
                    {
                        Title = ex.Message,
                        Body = string.IsNullOrEmpty(ex.StackTrace) ? "There is no stack trace for the error." : ex.StackTrace,
                    },
                },
            };
        }
    }

    private ListItem GetListItem(IDiscussion discussion)
    {
        return new ListItem(new LinkCommand(discussion.HtmlUrl, _resources))
        {
            Title = discussion.Title,
            Subtitle = discussion.RepositoryFullName,
            Icon = GitHubIcon.IconDictionary["Discussions"],
            MoreCommands = new CommandContextItem[]
            {
                new(new CopyCommand(discussion.HtmlUrl, _resources.GetResource("Commands_CopyURL"), _resources)),
            },
        };
    }
}
