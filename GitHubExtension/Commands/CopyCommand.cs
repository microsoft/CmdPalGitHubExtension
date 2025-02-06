// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Commands;

internal sealed partial class CopyCommand : InvokableCommand
{
    private readonly string _valueToCopy;

    internal CopyCommand()
    {
        _valueToCopy = "Nothing to copy";
        Name = "Copy to clipboard";
        Icon = new IconInfo("\uE8C8");
    }

    internal CopyCommand(string valueToCopy, string valueToCopyName)
    {
        _valueToCopy = valueToCopy;
        Name = $"Copy {valueToCopyName}";
        Icon = new IconInfo("\uE8C8");
    }

    public override CommandResult Invoke()
    {
        ClipboardHelper.SetText(_valueToCopy);
        return CommandResult.Dismiss();
    }
}
