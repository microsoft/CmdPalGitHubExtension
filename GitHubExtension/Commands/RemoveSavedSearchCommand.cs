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

    public static event TypedEventHandler<object, object?>? SearchRemoving;

    public static event TypedEventHandler<object, object?>? SearchRemoved;

    public RemoveSavedSearchCommand(SearchCandidate search)
    {
        savedSearch = search;
        Name = "Remove";
        Icon = new IconInfo("\uE8A7");
    }

    public RemoveSavedSearchCommand(PersistentData.Search search)
    {
        savedSearch = new SearchCandidate(search.SearchString, search.Name);
        Name = "Remove";
        Icon = new IconInfo("\uE8A7");
    }

    public override CommandResult Invoke()
    {
        SearchRemoving?.Invoke(this, null);
        SearchHelper.Instance.RemoveSavedSearch(savedSearch);
        SearchRemoved?.Invoke(this, null);

        return CommandResult.KeepOpen();
    }
}
