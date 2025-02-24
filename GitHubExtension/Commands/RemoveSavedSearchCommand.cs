// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Foundation;

namespace GitHubExtension.Commands;

public partial class RemoveSavedSearchCommand : InvokableCommand
{
    private readonly SearchCandidate savedSearch;

    private readonly ISearchHelper _searchHelper;

    public static event TypedEventHandler<object, object?>? SearchRemoving;

    public static event TypedEventHandler<object, object>? SearchRemoved;

    public RemoveSavedSearchCommand(SearchCandidate search, ISearchHelper searchHelper)
    {
        savedSearch = search;
        Name = "Remove";
        Icon = new IconInfo("\uecc9");
        _searchHelper = searchHelper;
    }

    public RemoveSavedSearchCommand(PersistentData.Search search, ISearchHelper searchHelper)
    {
        savedSearch = new SearchCandidate(search.SearchString, search.Name);
        Name = "Remove";
        Icon = new IconInfo("\uecc9");
        _searchHelper = searchHelper;
    }

    public override CommandResult Invoke()
    {
        try
        {
            var numSavedSearchesBeforeRemoval = _searchHelper.GetSavedSearches().Result.Count();
            SearchRemoving?.Invoke(this, null);
            _searchHelper.RemoveSavedSearch(savedSearch).Wait();
            SearchRemoved?.Invoke(this, numSavedSearchesBeforeRemoval > _searchHelper.GetSavedSearches().Result.Count());
        }
        catch (Exception ex)
        {
            SearchRemoved?.Invoke(this, ex);
        }

        return CommandResult.KeepOpen();
    }
}
