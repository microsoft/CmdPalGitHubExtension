// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
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

    private readonly SavedSearchesMediator _savedSearchesMediator;

    public SavedSearchesPage(
       ISearchPageFactory searchPageFactory,
       ISearchRepository searchRepository,
       IResources resources,
       IListItem addSearchListItem,
       SavedSearchesMediator savedSearchesMediator)
    {
        _resources = resources;
        Title = _resources.GetResource("Pages_Saved_Searches");
        Name = Title; // Name is for the command, Title is for the page
        Icon = GitHubIcon.IconDictionary["Search"];
        _savedSearchesMediator = savedSearchesMediator;
        _savedSearchesMediator.SearchRemoved += OnSearchRemoved;
        _savedSearchesMediator.SearchRemoving += OnSearchRemoving;
        _searchPageFactory = searchPageFactory;
        _searchRepository = searchRepository;
        _addSearchListItem = addSearchListItem;
        _savedSearchesMediator.SearchSaved += OnSearchSaved;
    }

    private void OnSearchRemoved(object? sender, SavedSearchRemovedEventArgs args)
    {
        IsLoading = false;

        if (args.Exception != null)
        {
            var toast = new ToastStatusMessage(new StatusMessage()
            {
                Message = $"{_resources.GetResource("Pages_Saved_Searches_Error")} {args.Exception.Message}",
                State = MessageState.Error,
            });

            toast.Show();
        }
        else if (args.Status && args.Search != null)
        {
            RaiseItemsChanged(0);
            ToastHelper.ShowToast($"{_resources.GetResource("Pages_Saved_Searches_RemovedSavedSearchSuccess")} {args.Search?.Name}", MessageState.Success);
        }
        else if (!args.Status)
        {
            var toast = new ToastStatusMessage(new StatusMessage()
            {
                Message = _resources.GetResource("Pages_Saved_Searches_Failure"),
                State = MessageState.Error,
            });

            toast.Show();
        }
    }

    private void OnSearchRemoving(object? sender, object? e)
    {
        IsLoading = true;
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
    public void OnSearchSaved(object? sender, object? args)
    {
        IsLoading = false;

        if (args != null && args is SearchCandidate)
        {
            RaiseItemsChanged(0);
        }

        // errors are handled in SaveSearchPage
    }
}
