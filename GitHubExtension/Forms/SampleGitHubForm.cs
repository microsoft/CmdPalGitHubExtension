// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Devices.Bluetooth.Advertisement;

namespace GitHubExtension.Forms;

internal sealed partial class SampleGitHubForm : GitHubForm
{
    public override Dictionary<string, string> TemplateSubstitutions => new()
    {
        { "{{SaveSearchFormTitle}}", "SaveSearch" },
        { "{{SavedSearchString}}", string.Empty },
        { "{{SavedSearchName}}", string.Empty },
    };

    public override string TemplateJson() => LoadTemplateJsonFromFile("SaveSearch");

    public SampleGitHubForm()
    {
        ExtensionHost.LogMessage("SampleGitHubForm constructor");
    }
}
