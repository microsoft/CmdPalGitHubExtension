// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
    private readonly IResources _resources;

    public SaveSearchPage(SaveSearchForm saveSearchForm, StatusMessage statusMessage, IResources resources)
    {
        _saveSearchForm = saveSearchForm;
        _statusMessage = statusMessage;
        _resources = resources;
        _successMessage = _resources.GetResource("Message_Search_Saved");
        _errorMessage = resources.GetResource("Message_Search_Saved_Error");
        Icon = new IconInfo("\uecc8");
        Title = _resources.GetResource("Pages_SaveSearch");
        Name = Title; // Title is for the Page, Name is for the Command

        FormEventHelper.WireFormEvents(_saveSearchForm, this, _statusMessage, _successMessage, _errorMessage);

        ExtensionHost.HideStatus(_statusMessage);
    }

    public override IContent[] GetContent()
    {
        ExtensionHost.HideStatus(_statusMessage);
        return [_saveSearchForm];
    }
}
