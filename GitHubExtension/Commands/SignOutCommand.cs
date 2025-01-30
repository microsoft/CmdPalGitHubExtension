﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using Microsoft.CmdPal.Extensions.Helpers;
using Windows.Foundation;

namespace GitHubExtension.Commands;

internal sealed partial class SignOutCommand : InvokableCommand
{
    internal event TypedEventHandler<object, SignInStatusChangedEventArgs>? SignOutAction;

    internal SignOutCommand()
    {
        Name = "Sign Out";
        Icon = new("\uE8A7");
    }

    public override CommandResult Invoke()
    {
        var authProvider = DeveloperIdProvider.GetInstance();
        var devIds = authProvider.GetLoggedInDeveloperIdsInternal();

        foreach (var devId in devIds)
        {
            authProvider.LogoutDeveloperId(devId);
        }

        SignOutAction?.Invoke(this, new SignInStatusChangedEventArgs(false));
        return CommandResult.GoHome();
    }
}
