// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Commands;

public partial class RemoveSavedSearchCommand : InvokableCommand
{
    private readonly ISearch savedSearch;
    private readonly ISearchRepository _searchRepository;
    private readonly IResources _resources;
    private readonly SavedSearchesMediator _savedSearchesMediator;

    public RemoveSavedSearchCommand(ISearch search, ISearchRepository searchRepository, IResources resources, SavedSearchesMediator savedSearchesMediator)
    {
        _searchRepository = searchRepository;
        _resources = resources;
        _savedSearchesMediator = savedSearchesMediator;

        savedSearch = new SearchCandidate(search.SearchString, search.Name);
        Name = _resources.GetResource("Commands_Remove_Saved_Search");
        Icon = new IconInfo("\uecc9");
    }

    public override CommandResult Invoke()
    {
        Task.Run(async () => await RemoveSavedSearch())
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    ExtensionHost.LogMessage(new LogMessage() { Message = $"Error removing saved search: {task.Exception?.GetBaseException().Message}" });
                    var args = new SavedSearchesUpdatedEventArgs(false, task.Exception, savedSearch);
                    _savedSearchesMediator.RemoveSearch(args);
                }
                else
                {
                    var args = new SavedSearchesUpdatedEventArgs(task.Result, null, savedSearch);
                    _savedSearchesMediator.RemoveSearch(args);
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
