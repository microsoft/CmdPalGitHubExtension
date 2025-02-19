// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Forms;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Pages;

internal sealed partial class EditSearchPage : FormPage
{
    private readonly SaveSearchForm _saveSearchForm = new();

    public EditSearchPage()
    {
        Icon = new IconInfo(string.Empty);
        Name = "Edit Search";
    }

    public override IForm[] Forms()
    {
        return new IForm[] { _saveSearchForm };
    }
}
