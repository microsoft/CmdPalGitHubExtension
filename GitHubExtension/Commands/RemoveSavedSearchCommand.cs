// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Helpers;
using GitHubExtension.Pages;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Foundation;

namespace GitHubExtension.Commands;

public partial class RemoveSavedSearchCommand : InvokableCommand
{
    private readonly SearchCandidate savedSearch;

    public static event TypedEventHandler<object, object?>? SearchRemoving;

    public static event TypedEventHandler<object, object>? SearchRemoved;

    public RemoveSavedSearchCommand(SearchCandidate search)
    {
        savedSearch = search;
        Name = "Remove";
        Icon = new IconInfo("\uecc9");
    }

    public RemoveSavedSearchCommand(ISearch search)
    {
        savedSearch = new SearchCandidate(search.SearchString, search.Name);
        Name = "Remove";
        Icon = new IconInfo("\uecc9");
    }

    public override CommandResult Invoke()
    {
        try
        {
            var numSavedSearchesBeforeRemoval = SearchHelper.Instance.GetSavedSearches().Result.Count();
            SearchRemoving?.Invoke(this, null);
            SearchHelper.Instance.RemoveSavedSearch(savedSearch).Wait();
            SearchRemoved?.Invoke(this, numSavedSearchesBeforeRemoval > SearchHelper.Instance.GetSavedSearches().Result.Count());
        }
        catch (Exception ex)
        {
            SearchRemoved?.Invoke(this, ex);
        }

        return CommandResult.KeepOpen();
    }
}
