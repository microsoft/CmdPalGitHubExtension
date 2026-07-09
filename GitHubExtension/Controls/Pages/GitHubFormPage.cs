// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls.Forms;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Pages;

// Reusable ContentPage host for the create/comment forms. Wires the form's
// submit/loading events to a StatusMessage and shows a success or error toast.
public sealed partial class GitHubFormPage : ContentPage
{
    private readonly FormContent _form;
    private readonly StatusMessage _statusMessage;

    public GitHubFormPage(FormContent form, IResources resources, string titleKey, string iconGlyph, string successKey, string errorKey)
    {
        _form = form;
        _statusMessage = new StatusMessage();

        Icon = new IconInfo(iconGlyph);
        Title = resources.GetResource(titleKey);
        Name = Title;

        FormEventHelper.WireFormEvents(
            (IGitHubForm)form,
            this,
            _statusMessage,
            resources.GetResource(successKey),
            resources.GetResource(errorKey));

        ExtensionHost.HideStatus(_statusMessage);
    }

    public override IContent[] GetContent()
    {
        ExtensionHost.HideStatus(_statusMessage);
        return [_form];
    }
}
