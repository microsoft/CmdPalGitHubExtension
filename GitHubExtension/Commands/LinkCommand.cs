// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using GitHubExtension.DataModel.DataObjects;
using Microsoft.CmdPal.Extensions.Helpers;

namespace GitHubExtension.Commands;

internal sealed partial class LinkCommand : InvokableCommand
{
    private readonly Issue _issue;

    internal LinkCommand()
    {
        _issue = new Issue();
        Name = "Open link";
        Icon = new("\uE8A7");
    }

    internal LinkCommand(Issue issue)
    {
        _issue = issue;
        Name = "Open link";
        Icon = new("\uE8A7");
    }

    public override CommandResult Invoke()
    {
        Process.Start(new ProcessStartInfo(_issue.HtmlUrl) { UseShellExecute = true });
        return CommandResult.KeepOpen();
    }
}
