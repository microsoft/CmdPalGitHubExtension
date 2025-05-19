// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls.Forms;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls;

internal sealed partial class EditSearchPage : ContentPage
{
    private readonly IResources _resources;
    private readonly SaveSearchForm _saveSearchForm;
    private readonly StatusMessage _statusMessage;
    private readonly string _successMessage;
    private readonly string _errorMessage;

    public EditSearchPage(IResources resources, SaveSearchForm saveSearchForm, StatusMessage statusMessage)
    {
        _resources = resources;
        _saveSearchForm = saveSearchForm;
        _statusMessage = statusMessage;
        _successMessage = _resources.GetResource("Pages_Search_Edited_Success");
        _errorMessage = _resources.GetResource("Pages_Search_Edited_Failed");

        FormEventHelper.WireFormEvents(_saveSearchForm, this, _statusMessage, _successMessage, _errorMessage);

        ExtensionHost.HideStatus(_statusMessage);

        Title = _resources.GetResource("Pages_Edit");
        Name = Title; // Title is for the Page, Name is for the Command
        Icon = new IconInfo("\ue70f");
    }

    public override IContent[] GetContent()
    {
        ExtensionHost.HideStatus(_statusMessage);
        return [_saveSearchForm];
    }
}
