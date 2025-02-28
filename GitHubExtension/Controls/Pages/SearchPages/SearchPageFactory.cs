// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls.Commands;
using GitHubExtension.Controls.Forms;
using GitHubExtension.DataModel.Enums;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Pages.SearchPages;

public class SearchPageFactory
{
    private readonly ICacheDataManager _cacheDataManager;
    private readonly ISearchRepository _searchRepository;

    public SearchPageFactory(ICacheDataManager cacheDataManager, ISearchRepository searchRepository)
    {
        _cacheDataManager = cacheDataManager;
        _searchRepository = searchRepository;
    }

    private ListPage CreatePageForSearch(ISearch search)
    {
        return search.Type switch
        {
            SearchType.PullRequests => new PullRequestsSearchPage(search, _cacheDataManager),
            SearchType.Issues => new IssuesSearchPage(search, _cacheDataManager),
            _ => throw new NotImplementedException(),
        };
    }

    public ListItem CreateItemForSearch(ISearch search)
    {
        return new ListItem(CreatePageForSearch(search))
        {
            Title = search.Name,
            Subtitle = search.SearchString,
            Icon = new IconInfo(GitHubIcon.IconDictionary[$"{search.Type}"]),
            MoreCommands = new CommandContextItem[]
            {
                new(new RemoveSavedSearchCommand(search, _searchRepository)),
                new(new EditSearchPage(search, new SaveSearchForm(search, _searchRepository), new StatusMessage(), "Search edited successfully!", "Error in editing search")),
            },
        };
    }
}
