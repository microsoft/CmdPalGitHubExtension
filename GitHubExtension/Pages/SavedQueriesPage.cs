// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension;

internal sealed partial class SavedQueriesPage : ListPage
{
    public SavedQueriesPage()
    {
        Icon = new IconInfo(string.Empty);
        Name = "Saved Queries";
    }

    public override IListItem[] GetItems()
    {
        return new ListItem[]
        {
            new(new NoOpCommand())
            {
                Title = "No queries found. Save queries from the main page.",
                Icon = new IconInfo(string.Empty),
            },
        };
    }
}
