// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataModel.DataObjects;
using GitHubExtension.Forms;
using GitHubExtension.Helpers;
using GitHubExtension.Pages;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension;

internal sealed partial class SavedSearchesPage : ListPage
{
#pragma warning disable IDE0044 // Add readonly modifier
    private List<SearchPage> _savedSearches;
#pragma warning restore IDE0044 // Add readonly modifier

    public SavedSearchesPage()
    {
        Icon = new IconInfo(string.Empty);
        Name = "Saved Searches";
        _savedSearches = new List<SearchPage>();
        SaveSearchForm.SearchSaved += OnSearchSaved;
    }

    public override IListItem[] GetItems()
    {
        if (_savedSearches.Count > 0)
        {
            var searches = _savedSearches.Select(savedSearch => new ListItem(savedSearch)
            {
                Title = savedSearch.Title,
                Icon = new IconInfo(GitHubIcon.IconDictionary[savedSearch.CurrentSearch.Type]),
            }).ToList();

            searches.Add(new(new SaveSearchPage())
            {
                Title = "Add a search",
                Icon = new IconInfo(string.Empty),
            });
            searches.Add(new(new SaveSearchPage(SearchInput.Survey))
            {
                Title = "Add a search (full form)",
                Icon = new IconInfo(string.Empty),
            });

            return searches.ToArray();
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
        else if (args != null && args is Search)
        {
            AddSearch((Search)args);
        }
    }

    private void AddSearch(Search search)
    {
        _savedSearches.Add(new SearchPage(search));
        RaiseItemsChanged(_savedSearches.Count + 1);
    }
}
