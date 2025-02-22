// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Client;
using GitHubExtension.DataManager;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Serilog;

namespace GitHubExtension;

internal abstract partial class SearchPage<T> : ListPage
{
    protected ILogger Logger { get; }

    private readonly ICacheManager _cacheManager;

    public PersistentData.Search CurrentSearch { get; private set; }

    // To avoid race condition between multiple requests
    private readonly object _requestLock = new();

    private DateTime LastRequested { get; set; } = DateTime.MinValue;

    private readonly TimeSpan _requestCooldown = TimeSpan.FromMinutes(5);

    // Search is mandatory for this page to exist
    protected SearchPage(PersistentData.Search search, ICacheManager cacheManager)
    {
        Icon = new IconInfo(GitHubIcon.IconDictionary[$"{search.Type}"]);
        Name = search.Name;
        CurrentSearch = search;
        _cacheManager = cacheManager;
        Logger = Log.ForContext("SourceContext", $"Pages/{GetType().Name}");
    }

    public override IListItem[] GetItems() => DoGetItems(SearchText).GetAwaiter().GetResult();

    protected async Task RequestContentData()
    {
        lock (_requestLock)
        {
            if (DateTime.UtcNow - LastRequested < _requestCooldown)
            {
                Logger.Information($"Too soon to request an update.");
                return;
            }

            LastRequested = DateTime.UtcNow;
        }

        await _cacheManager.Refresh(UpdateType.Search, CurrentSearch);
    }

    protected void CacheManagerUpdateHandler(object? source, CacheManagerUpdateEventArgs e)
    {
        if (e.Kind == CacheManagerUpdateKind.Updated)
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

    private async Task<IEnumerable<T>> GetSearchItemsAsync()
    {
        _cacheManager.OnUpdate += CacheManagerUpdateHandler;

        // To avoid locked database
        _cacheManager.CancelUpdateInProgress();
        var dataManager = _cacheManager.DataManager;
        var dsSearch = dataManager.GetSearch(CurrentSearch.Name, CurrentSearch!.SearchString);
        var items = await LoadContentData(dsSearch!);

        Logger.Information($"Found {items.Count()} items matching search query \"{CurrentSearch.Name}\"");

        _ = RequestContentData();

        return items;
    }

    protected abstract ListItem GetListItem(T item);

    protected abstract Task<IEnumerable<T>> LoadContentData(DataModel.Search dsSearch);

    protected static string GetOwner(string repositoryUrl) => Validation.ParseOwnerFromGitHubURL(repositoryUrl);

    protected static string GetRepo(string repositoryUrl) => Validation.ParseRepositoryFromGitHubURL(repositoryUrl);
}
