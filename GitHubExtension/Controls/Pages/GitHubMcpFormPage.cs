// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls.Forms;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Pages;

public sealed partial class GitHubMcpFormPage : ContentPage, IDisposable
{
    private readonly GitHubMcpForm _mcpForm;
    private readonly StatusMessage _statusMessage;
    private readonly IResources _resources;

    public GitHubMcpFormPage(GitHubMcpForm mcpForm, StatusMessage statusMessage, IResources resources)
    {
        _mcpForm = mcpForm;
        _statusMessage = statusMessage;
        _resources = resources;

        // Wire form events
        FormEventHelper.WireFormEvents(
            _mcpForm,
            this,
            _statusMessage,
            _resources.GetResource("Message_CreateTask_Success"),
            _resources.GetResource("Message_CreateTask_Failed"));
    }

    public override IContent[] GetContent() => [_mcpForm];

    // Disposing area
    private bool _disposed;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _mcpForm?.Dispose();
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
