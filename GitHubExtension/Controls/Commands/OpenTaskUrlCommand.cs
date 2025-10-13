// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Serilog;

namespace GitHubExtension.Controls.Commands;

/// <summary>
/// Command to open task URL in browser.
/// </summary>
internal sealed class OpenTaskUrlCommand : InvokableCommand
{
    private readonly string? _url;
    private readonly IResources _resources;
    private readonly ILogger _logger;

    internal OpenTaskUrlCommand(string? url, IResources resources)
    {
        _url = url;
        _resources = resources;
        _logger = Log.ForContext("SourceContext", nameof(OpenTaskUrlCommand));
        Name = _resources.GetResource("Commands_Open_Link");
        Icon = new IconInfo("\uE774");
    }

    public override CommandResult Invoke()
    {
        try
        {
            if (string.IsNullOrEmpty(_url))
            {
                ToastHelper.ShowErrorToast("No URL available for this task");
                return CommandResult.KeepOpen();
            }

            Process.Start(new ProcessStartInfo(_url) { UseShellExecute = true });
            ToastHelper.ShowSuccessToast("Opening task in browser...");

            return CommandResult.Dismiss();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to open task URL: {Url}", _url);
            ToastHelper.ShowErrorToast("Failed to open task in browser");
            return CommandResult.KeepOpen();
        }
    }
}
