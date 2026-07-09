// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Commands;

// Wraps a destructive command in a confirmation prompt. When the user confirms,
// the inner command runs. Used for irreversible actions such as merging or
// closing.
internal sealed partial class ConfirmedCommand : InvokableCommand
{
    private readonly InvokableCommand _innerCommand;
    private readonly string _confirmTitle;
    private readonly string _confirmDescription;

    internal ConfirmedCommand(InvokableCommand innerCommand, string name, IconInfo icon, string confirmTitle, string confirmDescription)
    {
        _innerCommand = innerCommand;
        _confirmTitle = confirmTitle;
        _confirmDescription = confirmDescription;
        Name = name;
        Icon = icon;
    }

    public override CommandResult Invoke()
    {
        return CommandResult.Confirm(new ConfirmationArgs
        {
            Title = _confirmTitle,
            Description = _confirmDescription,
            PrimaryCommand = _innerCommand,
            IsPrimaryCommandCritical = true,
        });
    }
}
