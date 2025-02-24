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
    public SavedSearchesPage()
    {
        Icon = new IconInfo("\ue74e");
        Name = "Saved Searches";
        SaveSearchForm.SearchSaved += OnSearchSaved;
        RemoveSavedSearchCommand.SearchRemoved += OnSearchRemoved;
        RemoveSavedSearchCommand.SearchRemoving += OnSearchRemoving;
    }

    public override IListItem[] GetItems()
    {
        var savedSearches = SearchHelper.Instance.GetSavedSearches().Result;
        if (savedSearches.Any())
        {
            var searchPages = savedSearches.Select(savedSearch => new ListItem(SearchPage.CreateForSearch(savedSearch))
            {
                Title = savedSearch.Name,
                Icon = new IconInfo(GitHubIcon.IconDictionary[$"{savedSearch.Type}"]),
                MoreCommands = new CommandContextItem[]
                {
                    new(new RemoveSavedSearchCommand(savedSearch))
                    {
                        Title = "Remove",
                        Icon = new IconInfo("\uecc9"),
                    },
                    new(new EditSearchPage(savedSearch, new SaveSearchForm(savedSearch), new StatusMessage(), "Search edited successfully!", "Error in editing search"))
                    {
                        Title = "Edit",
                        Icon = new IconInfo("\ue70f"),
                    },
                },
            }).ToList();

            searchPages.Add(new(new SaveSearchPage(new SaveSearchForm(), new StatusMessage(), "Search saved successfully!", "Error in saving search"))
            {
                Title = "Add a search",
                Icon = new IconInfo("\uecc8"),
            });
            searchPages.Add(new(new SaveSearchPage(new SaveSearchForm(SearchInput.Survey), new StatusMessage(), "Search saved successfully!", "Error in saving search"))
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
                new(new SaveSearchPage(new SaveSearchForm(SearchInput.Survey), new StatusMessage(), "Search saved successfully!", "Error in saving search"))
                {
                    Title = "Add a search (full form)",
                    Icon = new IconInfo(string.Empty),
                },
                new(new SaveSearchPage(new SaveSearchForm(), new StatusMessage(), "Search saved successfully!", "Error in saving search"))
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
            RaiseItemsChanged(SearchHelper.Instance.GetSavedSearches().Result.Count());
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
