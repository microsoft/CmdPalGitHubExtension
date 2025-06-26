// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Commands;

internal sealed partial class CopyCommand : InvokableCommand
{
    private readonly string _valueToCopy;
    private readonly IResources _resources;

    internal CopyCommand(string valueToCopy, string valueToCopyString, IResources resources)
    {
        _valueToCopy = valueToCopy;
        Name = valueToCopyString;
        Icon = new IconInfo("\uE8C8");
        _resources = resources;
    }

    public override CommandResult Invoke()
    {
        ClipboardHelper.SetText(_valueToCopy);

        ToastHelper.ShowSuccessToast(_resources.GetResource("Message_CopyCommand_Success"));

        Thread.Sleep(1500); // Wait for the toast to show before dismissing the command
        return CommandResult.Dismiss();
    }
}
