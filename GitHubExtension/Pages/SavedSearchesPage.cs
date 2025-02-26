// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Commands;
using GitHubExtension.Forms;
using GitHubExtension.Helpers;
using GitHubExtension.Pages;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension;

public sealed partial class SavedSearchesPage : ListPage
{
    private readonly ListItem _addSearchListItem;

    private readonly ListItem _addSearchFullFormListItem;

    private readonly SearchPageFactory _searchPageFactory;

    private readonly ISearchRepository _searchRepository;

    public SavedSearchesPage(
        SearchPageFactory searchPageFactory,
        ISearchRepository searchRepository,
        AddSearchListItem addSearchListItem,
        AddSearchFullFormListItem addSearchFullFormListItem)
    {
        Icon = new IconInfo("\ue721");
        Name = "Saved GitHub Searches";
        SaveSearchForm.SearchSaved += OnSearchSaved;
        RemoveSavedSearchCommand.SearchRemoved += OnSearchRemoved;
        RemoveSavedSearchCommand.SearchRemoving += OnSearchRemoving;
        _searchPageFactory = searchPageFactory;
        _searchRepository = searchRepository;
        _addSearchListItem = addSearchListItem;
        _addSearchFullFormListItem = addSearchFullFormListItem;
    }

    public override IListItem[] GetItems()
    {
        var savedSearches = _searchRepository.GetSavedSearches().Result;
        if (savedSearches.Any())
        {
            var searchPages = savedSearches.Select(savedSearch => _searchPageFactory.CreateItemForSearch(savedSearch)).ToList();

            searchPages.Add(_addSearchListItem);
            searchPages.Add(_addSearchFullFormListItem);

            return searchPages.ToArray();
        }
        else
        {
            return [_addSearchListItem, _addSearchFullFormListItem];
        }
    }

    private void OnSearchSaved(object sender, object? args)
    {
        IsLoading = false;

        if (args != null && args is SearchCandidate)
        {
            RaiseItemsChanged(_searchRepository.GetSavedSearches().Result.Count());
        }

        // errors are handled in SaveSearchPage
    }

    private void OnSearchRemoved(object sender, object? args)
    {
        IsLoading = false;

        if (args is Exception e)
        {
            var toast = new ToastStatusMessage(new StatusMessage()
            {
                Message = $"Error in removing search: {e.Message}",
                State = MessageState.Error,
            });

            toast.Show();
        }
        else if (args is true)
        {
            RaiseItemsChanged(_searchRepository.GetSavedSearches().Result.Count());
        }
        else if (args is false)
        {
            var toast = new ToastStatusMessage(new StatusMessage()
            {
                Message = "Failed to remove search.",
                State = MessageState.Error,
            });

            toast.Show();
        }
    }

    private void OnSearchRemoving(object sender, object? args)
    {
        IsLoading = true;
    }
}
