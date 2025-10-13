// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls.Forms;
using GitHubExtension.Helpers;
using GitHubExtension.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Pages;

public sealed partial class CopilotChatPage : ContentPage, IDisposable
{
    private readonly IResources _resources;
    private readonly CopilotChatForm _copilotChatForm;

    public CopilotChatPage(IResources resources, IGitHubCopilotService copilotService)
    {
        _resources = resources;
        _copilotChatForm = new CopilotChatForm(copilotService, resources);

        Icon = GitHubIcon.IconDictionary["logo"];
        Name = _resources.GetResource("Commands_GitHub_Copilot");
        Commands = Array.Empty<CommandContextItem>();
    }

    public override IContent[] GetContent()
    {
        return [_copilotChatForm];
    }

    private bool _disposed;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _copilotChatForm?.Dispose();
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
