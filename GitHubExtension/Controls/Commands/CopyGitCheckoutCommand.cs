// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Commands;

internal sealed partial class CopyGitCheckoutCommand : InvokableCommand
{
    private readonly IPullRequest _pullRequestSource;

    internal CopyGitCheckoutCommand(IPullRequest pullRequestSource, string valueToCopyString)
    {
        _pullRequestSource = pullRequestSource;
        Name = valueToCopyString;
        Icon = new IconInfo("\uE8C8");
    }

    public override CommandResult Invoke()
    {
        ClipboardHelper.SetText($"git checkout {_pullRequestSource.SourceBranch}");
        return CommandResult.Dismiss();
    }
}
