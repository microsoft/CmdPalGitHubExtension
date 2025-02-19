// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataModel.Enums;
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
        Icon = new IconInfo(string.Empty);
        Name = "Saved Searches";
        SaveSearchForm.SearchSaved += OnSearchSaved;
    }

    public override IListItem[] GetItems()
    {
        // Maybe this should be awaited and the method async
        var savedSearches = SearchHelper.Instance.GetSavedSearches().Result;
        if (savedSearches.Any())
        {
            var searchPages = savedSearches.Select(savedSearch => new ListItem(new SearchPage(savedSearch))
            {
                Title = savedSearch.Name,
                Icon = new IconInfo(GitHubIcon.IconDictionary[$"{savedSearch.Type}"]),
            }).ToList();

            searchPages.Add(new(new SaveSearchPage())
            {
                Title = "Add a search",
                Icon = new IconInfo(string.Empty),
            });
            searchPages.Add(new(new SaveSearchPage(SearchInput.Survey))
            {
                Title = "Add a search (full form)",
                Icon = new IconInfo(string.Empty),
            });

            return searchPages.ToArray();
        }
        else
        {
            return new ListItem[]
            {
                new(new SaveSearchPage(SearchInput.Survey))
                {
                    Title = "Add a search (full form)",
                    Icon = new IconInfo(string.Empty),
                },
                new(new SaveSearchPage())
                {
                    Title = "Add a search by string",
                    Icon = new IconInfo(string.Empty),
                },
            };
        }
    }

    private void OnSearchSaved(object sender, object? args)
    {
        if (args is Exception)
        {
            // do nothing
        }
        else if (args != null && args is SearchCandidate)
        {
            RaiseItemsChanged(SearchHelper.Instance.GetSavedSearches().Result.Count());
        }
    }
}
