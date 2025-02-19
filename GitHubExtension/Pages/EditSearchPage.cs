// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Forms;
using GitHubExtension.PersistentData;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Pages;

internal sealed partial class EditSearchPage : FormPage
{
    private readonly Search _searchToEdit;

    public EditSearchPage(Search searchToEdit)
    {
        Icon = new IconInfo(string.Empty);
        Name = "Edit Search";
        _searchToEdit = searchToEdit;
    }

    public override IForm[] Forms()
    {
        return new IForm[] { new SaveSearchForm(_searchToEdit) };
    }
}
