// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls.Forms;
using GitHubExtension.Helpers;
using GitHubExtension.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Pages;

public sealed partial class GitHubCopilotFormPage : ContentPage, IDisposable
{
    private readonly IResources _resources;
    private readonly GitHubCopilotForm _copilotForm;

    public GitHubCopilotFormPage(IResources resources, IGitHubCopilotService copilotService)
    {
        _resources = resources;
        _copilotForm = new GitHubCopilotForm(copilotService, resources);

        Icon = GitHubIcon.IconDictionary["logo"];
        Name = _resources.GetResource("Commands_GitHub_Copilot_CreateTask");
        Commands = Array.Empty<CommandContextItem>();
    }

    public override IContent[] GetContent()
    {
        return [_copilotForm];
    }

    private bool _disposed;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _copilotForm?.Dispose();
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
#pragma warning restore SA1518
