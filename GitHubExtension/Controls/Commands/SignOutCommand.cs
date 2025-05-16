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
    private readonly IResources _resources;
    private readonly IDeveloperIdProvider _developerIdProvider;
    private readonly AuthenticationMediator _authenticationMediator;
    private bool _invoked;

    public SignOutCommand(IResources resources, IDeveloperIdProvider developerIdProvider, AuthenticationMediator authenticationMediator)
    {
        _resources = resources;
        _developerIdProvider = developerIdProvider;
        _authenticationMediator = authenticationMediator;
        _authenticationMediator.SignInAction += ResetCommand;
        _authenticationMediator.SignOutAction += ResetCommand;
        Name = _resources.GetResource("Forms_Sign_Out_Button_Title");
        Icon = GitHubIcon.IconDictionary["logo"];
        _invoked = false;
    }

    private void ResetCommand(object? sender, SignInStatusChangedEventArgs e)
    {
        _invoked = !e.IsSignedIn;
    }

    public override CommandResult Invoke()
    {
        if (_invoked)
        {
            return CommandResult.KeepOpen();
        }

        Task.Run(() =>
        {
            _invoked = true;
            _authenticationMediator.SetLoadingState(true);
            try
            {
                var devIds = _developerIdProvider.GetLoggedInDeveloperIdsInternal();

                foreach (var devId in devIds)
                {
                    _developerIdProvider.LogoutDeveloperId(devId);
                }

                var signOutSucceeded = !_developerIdProvider.GetLoggedInDeveloperIdsInternal().Any();

                _authenticationMediator.SetLoadingState(false);
                _authenticationMediator.SignOut(new SignInStatusChangedEventArgs(!signOutSucceeded, null));
                ToastHelper.ShowToast(_resources.GetResource("Message_Sign_Out_Success"), MessageState.Success);
            }
            catch (Exception ex)
            {
                _authenticationMediator.SetLoadingState(false);

                // if sign out fails, the user is still signed in (true)
                _authenticationMediator.SignOut(new SignInStatusChangedEventArgs(true, ex));
                ToastHelper.ShowToast($"{_resources.GetResource("Message_Sign_Out_Fail")} {ex.Message}", MessageState.Error);
            }
        });
        return CommandResult.KeepOpen();
    }
}
