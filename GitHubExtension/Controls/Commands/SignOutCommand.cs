// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Commands;

public class SignOutCommand : InvokableCommand
{
    private readonly IDeveloperIdProvider _developerIdProvider;
    private readonly IResources _resources;
    private readonly AuthenticationMediator _authenticationMediator;
    private readonly string _successMessage;
    private readonly string _failureMessage;
    private readonly StatusMessage _statusMessage;

    public SignOutCommand(IDeveloperIdProvider developerIdProvider, IResources resources, StatusMessage statusMessage, AuthenticationMediator authenticationMediator)
    {
        Name = "Sign Out";
        _resources = resources;
        _developerIdProvider = developerIdProvider;
        _authenticationMediator = authenticationMediator;
        _successMessage = _resources.GetResource("Message_Sign_In_Success");
        _failureMessage = _resources.GetResource("Message_Sign_In_Fail");
        _statusMessage = statusMessage;
        Name = _resources.GetResource("Forms_Sign_In");
        Icon = GitHubIcon.IconDictionary["logo"];
    }

    public override ICommandResult Invoke()
    {
        try
        {
            var devIds = _developerIdProvider.GetLoggedInDeveloperIdsInternal();

            foreach (var devId in devIds)
            {
                _developerIdProvider.LogoutDeveloperId(devId);
            }

            var signOutSucceeded = !_developerIdProvider.GetLoggedInDeveloperIdsInternal().Any();

            _authenticationMediator.SignOut(new SignInStatusChangedEventArgs(!signOutSucceeded, null));
            EventHelper.RaiseToast(_statusMessage, _successMessage, _failureMessage, null, signOutSucceeded);
        }
        catch (Exception ex)
        {
            // if sign out fails, the user is still signed in (true)
            _authenticationMediator.SignOut(new SignInStatusChangedEventArgs(true, ex));
            EventHelper.RaiseToast(_statusMessage, _successMessage, _failureMessage, ex, false);
        }

        return CommandResult.KeepOpen();
    }
}
