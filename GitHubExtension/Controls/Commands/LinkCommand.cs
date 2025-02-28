// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using GitHubExtension.Controls.Pages;
using GitHubExtension.DataModel;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Commands;

internal sealed partial class LinkCommand : InvokableCommand
{
    private readonly string _htmlUrl;

    internal LinkCommand(IIssue issue)
    {
        _htmlUrl = issue.HtmlUrl;
        Name = "Open link";
        Icon = new IconInfo("\uE8A7");
    }

    internal LinkCommand(IPullRequest pullRequest)
    {
        _htmlUrl = pullRequest.HtmlUrl;
        Name = "Open link";
        Icon = new IconInfo("\uE8A7");
    }

    public override CommandResult Invoke()
    {
        Process.Start(new ProcessStartInfo(_htmlUrl) { UseShellExecute = true });
        return CommandResult.KeepOpen();
    }
}
