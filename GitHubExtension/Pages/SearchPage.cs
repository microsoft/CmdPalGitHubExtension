// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Client;
using GitHubExtension.Commands;
using GitHubExtension.DataManager;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Serilog;

namespace GitHubExtension;

internal abstract partial class SearchPage : ListPage
{
    protected ILogger Logger { get; }

    public PersistentData.Search CurrentSearch { get; private set; }

    private DateTime LastRequested { get; set; } = DateTime.MinValue;

    private readonly TimeSpan _requestCooldown = TimeSpan.FromMinutes(5);

    // Search is mandatory for this page to exist
    protected SearchPage(PersistentData.Search search)
    {
        Icon = new IconInfo(GitHubIcon.IconDictionary[$"{search.Type}"]);
        Name = search.Name;
        CurrentSearch = search;
        Logger = Log.ForContext("SourceContext", $"Pages/{GetType().Name}");
    }

    public static SearchPage CreateForSearch(PersistentData.Search search)
    {
        switch (search.Type)
        {
            case DataModel.Enums.SearchType.Issues:
                return new IssuesSearchPage(search);
            case DataModel.Enums.SearchType.PullRequests:
                return new PullRequestsSearchPage(search);
            default:
                throw new NotImplementedException($"Search type {search.Type} is not implemented.");
        }
    }

    public override IListItem[] GetItems() => DoGetItems(SearchText).GetAwaiter().GetResult();

    protected async Task RequestContentData()
    {
        if (DateTime.UtcNow - LastRequested < _requestCooldown)
        {
            Logger.Information($"Too soon to request an update.");
            return;
        }

        var cacheManager = CacheManager.GetInstance();
        await cacheManager.Refresh(UpdateType.Search, CurrentSearch);
        LastRequested = DateTime.UtcNow;
    }

    protected void CacheManagerUpdateHandler(object? source, CacheManagerUpdateEventArgs e)
    {
        if (e.Kind == CacheManagerUpdateKind.Updated)
        {
            Logger.Information($"Received cache manager update event.");
            RaiseItemsChanged(0);
        }
    }

    protected abstract Task<IListItem[]> DoGetItems(string query);

    protected static string GetOwner(string repositoryUrl) => Validation.ParseOwnerFromGitHubURL(repositoryUrl);

    protected static string GetRepo(string repositoryUrl) => Validation.ParseRepositoryFromGitHubURL(repositoryUrl);
}
