// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Commands;

public partial class RemoveSavedSearchCommand : InvokableCommand
{
    private readonly SearchCandidate savedSearch;

    public RemoveSavedSearchCommand(SearchCandidate search)
    {
        savedSearch = search;
        Name = "Remove";
        Icon = new IconInfo("\uE8A7");
    }

    public override CommandResult Invoke()
    {
        SearchHelper.Instance.RemoveSavedSearch(savedSearch);

        return CommandResult.KeepOpen();
    }
}
