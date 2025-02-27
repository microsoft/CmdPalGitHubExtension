// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Helpers;
using GitHubExtension.Pages;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Forms;

public partial class AddSearchListItem : ListItem
{
    public AddSearchListItem(SaveSearchPage page)

    // : base(new SaveSearchPage(new SaveSearchForm(SearchInput.SearchString), new StatusMessage(), "Search saved successfully!", "Error in saving search"))
    : base(page)
    {
        Title = "Add a search";
        Icon = new IconInfo("\uecc8");
    }
}
