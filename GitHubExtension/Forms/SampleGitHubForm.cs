// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;

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

    public override void HandleSubmit(string payload)
    {
        try
        {
            Task.Delay(3000).Wait();
            throw new InvalidOperationException();
        }
        catch (Exception ex)
        {
            RaiseLoadingStateChanged(false);
            RaiseFormSubmitted(new FormSubmitEventArgs(false, ex));
        }
    }

    public SampleGitHubForm()
    {
        ExtensionHost.LogMessage("SampleGitHubForm constructor");
    }
}
