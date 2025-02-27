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
    private readonly ISearch savedSearch;

    private readonly ISearchRepository _searchRepository;

    public static event TypedEventHandler<object, object?>? SearchRemoving;

    public static event TypedEventHandler<object, object>? SearchRemoved;

    public RemoveSavedSearchCommand(ISearch search, ISearchRepository searchRepository)
    {
        savedSearch = new SearchCandidate(search.SearchString, search.Name);
        Name = "Remove";
        Icon = new IconInfo("\uecc9");
        _searchRepository = searchRepository;
    }

    public override CommandResult Invoke()
    {
        try
        {
            var numSavedSearchesBeforeRemoval = _searchRepository.GetSavedSearches().Result.Count();
            SearchRemoving?.Invoke(this, null);
            _searchRepository.RemoveSavedSearch(savedSearch).Wait();
            SearchRemoved?.Invoke(this, numSavedSearchesBeforeRemoval > _searchRepository.GetSavedSearches().Result.Count());
        }
        catch (Exception ex)
        {
            SearchRemoved?.Invoke(this, ex);
        }

        return CommandResult.KeepOpen();
    }
}
