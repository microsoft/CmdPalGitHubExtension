// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls.Pages;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Foundation;

namespace GitHubExtension.Controls.Commands;

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
        SearchRemoving?.Invoke(this, null);
        Task.Run(async () => await RemoveSavedSearch())
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    SearchRemoved?.Invoke(this, task.Exception);
                }
                else
                {
                    SearchRemoved?.Invoke(this, task.Result);
                }
            });

        return CommandResult.KeepOpen();
    }

    private async Task<bool> RemoveSavedSearch()
    {
        var savedSearches = await _searchRepository.GetSavedSearches();
        var numSavedSearchesBeforeRemoval = savedSearches.Count();
        await _searchRepository.UpdateSearchTopLevelStatus(savedSearch, false);
        await _searchRepository.RemoveSavedSearch(savedSearch);
        return numSavedSearchesBeforeRemoval > (await _searchRepository.GetSavedSearches()).Count();
    }
}
