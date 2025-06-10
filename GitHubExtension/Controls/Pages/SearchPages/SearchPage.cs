// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Drawing;
using GitHubExtension.Client;
using GitHubExtension.DataManager.Cache;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Serilog;

namespace GitHubExtension.Controls.Pages;

public abstract partial class SearchPage<T> : ListPage
{
    protected ILogger Logger { get; }

    public ISearch CurrentSearch { get; private set; }

    protected ICacheDataManager CacheDataManager { get; private set; }

    protected IResources Resources { get; private set; }

    // Search is mandatory for this page to exist
    protected SearchPage(ISearch search, ICacheDataManager cacheDataManager, IResources resources)
    {
        Icon = GitHubIcon.IconDictionary[$"{search.Type}"];
        Name = search.Name;
        CurrentSearch = search;
        Logger = Log.ForContext("SourceContext", $"Pages/{GetType().Name}");
        CacheDataManager = cacheDataManager;
        Resources = resources;

        CacheDataManager.OnUpdateWeak += CacheManagerUpdateHandler;
    }

    public override IListItem[] GetItems() => DoGetItems(SearchText).GetAwaiter().GetResult();

    private void CacheManagerUpdateHandler(object? source, CacheManagerUpdateEventArgs e)
    {
        if (e.Kind == CacheManagerUpdateKind.Updated && (e.Search == null || e.Search == CurrentSearch))
        {
            Logger.Information($"Received cache manager update event.");
            RaiseItemsChanged(0);
        }
    }

    private async Task<IListItem[]> DoGetItems(string query)
    {
        try
        {
            Logger.Information($"Getting items for search query \"{CurrentSearch.Name}\"");
            var items = await GetSearchItemsAsync();

            var iconString = $"{CurrentSearch.Type}";

            if (items.Any())
            {
                return items.Select(item => GetListItem(item)).ToArray();
            }
            else
            {
                return new ListItem[]
                {
                    new(new NoOpCommand())
                    {
                        Title = Resources.GetResource("Pages_No_Items_Found"),
                        Icon = GitHubIcon.IconDictionary[iconString],
                    },
                };
            }
        }
        catch (Exception ex)
        {
            return
            [
                    new ListItem(new NoOpCommand())
                    {
                        Title = Resources.GetResource("Pages_Error_Title"),
                        Details = new Details()
                        {
                            Title = ex.Message,
                            Body = string.IsNullOrEmpty(ex.StackTrace) ? "There is no stack trace for the error." : ex.StackTrace,
                        },
                    },
            ];
        }
    }

    private async Task<IEnumerable<T>> GetSearchItemsAsync()
    {
        var items = await LoadContentData();

        Logger.Information($"Found {items.Count()} items matching search query \"{CurrentSearch.Name}\"");

        return items;
    }

    private Microsoft.CommandPalette.Extensions.Color GetFontColor(string colorStr)
    {
        var color = ColorTranslator.FromHtml($"#{colorStr}");

        // Luminance is a measure of the brightness of a color. It is a weighted sum of its RGB components.
        var luminance = (0.2126 * color.R) + (0.7152 * color.G) + (0.0722 * color.B);

        // If the luminance is greater than 128, the color is light, so use black font color.
        // Otherwise, use white font color.
        var fontColor = luminance > 128 ? System.Drawing.Color.Black : System.Drawing.Color.White;

        return new Microsoft.CommandPalette.Extensions.Color(fontColor.R, fontColor.G, fontColor.B, fontColor.A);
    }

    // IPullRequest implements IIssue
    protected ITag[] GetTags(IIssue item)
    {
        var tags = new List<ITag>();
        if (item.Labels != null)
        {
            // Limit to 4 tags for UI space
            var labels = item.Labels.Take(4);
            foreach (var label in labels)
            {
                var color = ColorTranslator.FromHtml($"#{label.Color}");
                tags.Add(new Tag
                {
                    Background = new(true, new Microsoft.CommandPalette.Extensions.Color(color.R, color.G, color.B, color.A)),
                    Foreground = new(true, GetFontColor(label.Color)),
                    Text = label.Name,
                });
            }
        }

        return tags.ToArray();
    }

    protected abstract ListItem GetListItem(T item);

    protected abstract Task<IEnumerable<T>> LoadContentData();

    protected static string GetOwner(string repositoryUrl) => Validation.ParseOwnerFromGitHubURL(repositoryUrl);

    protected static string GetRepo(string repositoryUrl) => Validation.ParseRepositoryFromGitHubURL(repositoryUrl);
}
