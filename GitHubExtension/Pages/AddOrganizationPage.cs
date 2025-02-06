// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Forms;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Pages;

internal sealed partial class AddOrganizationPage : FormPage
{
    private readonly AddOrganizationForm _addOrgForm = new();

    public AddOrganizationPage()
    {
        _addOrgForm.OrganizationAdded += OnOrganizationAdded;
    }

    public override IForm[] Forms() => new IForm[] { _addOrgForm };

    private void OnOrganizationAdded(object sender, object? args)
    {
        if (args is Exception ex)
        {
            var message = new StatusMessage() { Message = $"Error in adding organization: {ex.Message}", State = MessageState.Error };
            ExtensionHost.Host?.ShowStatus(message);
        }
        else
        {
            var message = new StatusMessage() { Message = "Organization added successfully!", State = MessageState.Success };
            ExtensionHost.Host?.ShowStatus(message);
        }
    }
}
