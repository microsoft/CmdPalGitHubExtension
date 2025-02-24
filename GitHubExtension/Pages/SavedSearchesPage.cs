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

internal sealed partial class SavedSearchesPage : ListPage
{
    private readonly ISearchHelper _searchHelper;
    private readonly IPagesFactory _pagesFactory;

    public SavedSearchesPage(ISearchHelper searchHelper, IPagesFactory pagesFactory)
    {
        Icon = new IconInfo("\ue74e");
        Name = "Saved Searches";

        // I still don't know how I feel about static events
        SaveSearchForm.SearchSaved += OnSearchSaved;
        SaveSearchForm.SearchSaving += OnSearchSaving;
        RemoveSavedSearchCommand.SearchRemoved += OnSearchRemoved;
        RemoveSavedSearchCommand.SearchRemoving += OnSearchRemoving;
        _searchHelper = searchHelper;
        _pagesFactory = pagesFactory;
    }

    public override IListItem[] GetItems()
    {
        var savedSearches = SearchHelper.Instance.GetSavedSearches().Result;
        if (savedSearches.Any())
        {
            var searchPages = savedSearches.Select(savedSearch => new ListItem(_pagesFactory.CreateForSearch(savedSearch))
            {
                Title = savedSearch.Name,
                Icon = new IconInfo(GitHubIcon.IconDictionary[$"{savedSearch.Type}"]),
                MoreCommands = new CommandContextItem[]
                {
                    new(_pagesFactory.GetRemoveSavedSearchCommand(savedSearch))
                    {
                        Title = "Remove",
                        Icon = new IconInfo("\uecc9"),
                    },
                    new(_pagesFactory.GetEditSearchPage(savedSearch))
                    {
                        Title = "Edit",
                        Icon = new IconInfo("\ue70f"),
                    },
                },
            }).ToList();

            searchPages.Add(new(_pagesFactory.GetSaveSearchPage())
            {
                Title = "Add a search",
                Icon = new IconInfo("\uecc8"),
            });
            searchPages.Add(new(_pagesFactory.GetSaveSearchPage(SearchInput.Survey))
            {
                Title = "Add a search (full form)",
                Icon = new IconInfo("\uecc8"),
            });

            return searchPages.ToArray();
        }
        else
        {
            return new ListItem[]
            {
                new(_pagesFactory.GetSaveSearchPage(SearchInput.Survey))
                {
                    Title = "Add a search (full form)",
                    Icon = new IconInfo(string.Empty),
                },
                new(_pagesFactory.GetSaveSearchPage())
                {
                    Title = "Add a search by string",
                    Icon = new IconInfo(string.Empty),
                },
            };
        }
    }

    private void OnSearchSaved(object sender, object? args)
    {
        IsLoading = false;

        if (args != null && args is SearchCandidate)
        {
            RaiseItemsChanged(_searchHelper.GetSavedSearches().Result.Count());
        }

        // errors are handled in SaveSearchPage
    }

    private void OnSearchSaving(object sender, bool args)
    {
        IsLoading = true;
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
            RaiseItemsChanged(SearchHelper.Instance.GetSavedSearches().Result.Count());
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
