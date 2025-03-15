// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Resources;
using GitHubExtension.Controls.Forms;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Pages;

public sealed partial class SaveSearchPage : ContentPage
{
    private readonly SaveSearchForm _saveSearchForm;
    private readonly StatusMessage _statusMessage;
    private readonly string _successMessage;
    private readonly string _errorMessage;

    public SaveSearchPage(SaveSearchForm saveSearchForm, StatusMessage statusMessage, string successMessage, string errorMessage, IResources resources)
    {
        _saveSearchForm = saveSearchForm;
        _statusMessage = statusMessage;
        _successMessage = successMessage;
        _errorMessage = errorMessage;
        Icon = new IconInfo("\uecc8");
        Title = resources.GetResource("ListItems_AddSearch");

        // Wire up events using the helper
        FormEventHelper.WireFormEvents(_saveSearchForm, this, _statusMessage, _successMessage, _errorMessage);

        // Hide status message initially
        ExtensionHost.HideStatus(_statusMessage);
    }

    public override IContent[] GetContent()
    {
        ExtensionHost.HideStatus(_statusMessage);
        return [_saveSearchForm];
    }
}
