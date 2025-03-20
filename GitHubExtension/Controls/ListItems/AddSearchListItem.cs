// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls.Pages;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.ListItems;

public partial class AddSearchListItem : ListItem
{
    public AddSearchListItem(SaveSearchPage page, IResources resources)

    // : base(new SaveSearchPage(new SaveSearchForm(SearchInput.SearchString), new StatusMessage(), "Search saved successfully!", "Error in saving search"))
    : base(page)
    {
        Title = resources.GetResource("ListItems_AddSearch");
        Icon = new IconInfo("\uecc8");
    }
}
