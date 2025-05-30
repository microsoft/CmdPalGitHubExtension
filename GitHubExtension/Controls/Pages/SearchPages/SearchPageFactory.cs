﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls.Commands;
using GitHubExtension.Controls.Forms;
using GitHubExtension.DataModel.Enums;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Pages;

public class SearchPageFactory : ISearchPageFactory
{
    private readonly ICacheDataManager _cacheDataManager;
    private readonly ISearchRepository _searchRepository;
    private readonly IResources _resources;
    private readonly SavedSearchesMediator _savedSearchesMediator;

    public SearchPageFactory(ICacheDataManager cacheDataManager, ISearchRepository searchRepository, IResources resources, SavedSearchesMediator savedSearchesMediator)
    {
        _cacheDataManager = cacheDataManager;
        _searchRepository = searchRepository;
        _resources = resources;
        _savedSearchesMediator = savedSearchesMediator;
    }

    private ListPage CreatePageForSearch(ISearch search)
    {
        return search.Type switch
        {
            SearchType.PullRequests => new PullRequestsSearchPage(search, _cacheDataManager, _resources),
            SearchType.Issues => new IssuesSearchPage(search, _cacheDataManager, _resources),
            _ => new CombinedSearchPage(search, _cacheDataManager, _resources),
        };
    }

    public IListItem CreateItemForSearch(ISearch search)
    {
        return new ListItem(CreatePageForSearch(search))
        {
            Title = search.Name,
            Subtitle = search.SearchString,
            Icon = GitHubIcon.IconDictionary[$"{search.Type}"],
            MoreCommands = new CommandContextItem[]
            {
                new(new RemoveSavedSearchCommand(search, _searchRepository, _resources, _savedSearchesMediator)),
                new(new EditSearchPage(
                    _resources,
                    new SaveSearchForm(search, _searchRepository, _resources, _savedSearchesMediator),
                    new StatusMessage())),
            },
        };
    }
}
