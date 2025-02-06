// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Helpers;
using GitHubExtension.Pages;
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
            new(new QueryPage())
            {
                Title = "Sample Query: Search GitHub Issues",
                Icon = new IconInfo(GitHubIcon.IconDictionary["issue"]),
            },
            new(new SaveQueryPage())
            {
                Title = "Add a query",
                Icon = new IconInfo(string.Empty),
            },
        };
    }
}
