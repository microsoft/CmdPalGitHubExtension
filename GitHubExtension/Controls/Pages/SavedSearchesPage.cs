// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.Controls.Commands;
using GitHubExtension.Controls.Forms;
using GitHubExtension.Controls.Pages;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension;

public partial class SavedSearchesPage : ListPage
{
    private readonly IListItem _addSearchListItem;

    private readonly ISearchPageFactory _searchPageFactory;

    private readonly ISearchRepository _searchRepository;
    private readonly IResources _resources;

    public SavedSearchesPage(
       ISearchPageFactory searchPageFactory,
       ISearchRepository searchRepository,
       IResources resources,
       IListItem addSearchListItem)
    {
        _resources = resources;

        Icon = new IconInfo("\ue721");
        Name = _resources.GetResource("Pages_Saved_Searches");
        SaveSearchForm.SearchSaved += OnSearchSaved;
        RemoveSavedSearchCommand.SearchRemoved += OnSearchRemoved;
        RemoveSavedSearchCommand.SearchRemoving += OnSearchRemoving;
        _searchPageFactory = searchPageFactory;
        _searchRepository = searchRepository;
        _addSearchListItem = addSearchListItem;
    }

    public override IListItem[] GetItems()
    {
        var savedSearches = _searchRepository.GetSavedSearches().Result;
        if (savedSearches.Any())
        {
            var searchPages = savedSearches.Select(savedSearch => _searchPageFactory.CreateItemForSearch(savedSearch)).ToList();

            searchPages.Add(_addSearchListItem);

            return searchPages.ToArray();
        }
        else
        {
            return [_addSearchListItem];
        }
    }

    // Change this to public to facilitate tests. As the event handler is
    // listening to a static event, it is not possible to mock the event.
    public void OnSearchSaved(object sender, object? args)
    {
        IsLoading = false;

        if (args != null && args is SearchCandidate)
        {
            RaiseItemsChanged(_searchRepository.GetSavedSearches().Result.Count());
        }

        // errors are handled in SaveSearchPage
    }

    public void OnSearchRemoved(object sender, object? args)
    {
        IsLoading = false;

        if (args is Exception e)
        {
            var toast = new ToastStatusMessage(new StatusMessage()
            {
                Message = $"{_resources.GetResource("Pages_Saved_Searches_Error")} {e.Message}",
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
                Message = _resources.GetResource("Pages_Saved_Searches_Failure"),
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
