// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Commands;

public class SignInCommand : InvokableCommand
{
    private readonly IResources _resources;
    private readonly IDeveloperIdProvider _developerIdProvider;
    private readonly AuthenticationMediator _authenticationMediator;
    private bool _invoked;

    public SignInCommand(IResources resources, IDeveloperIdProvider developerIdProvider, AuthenticationMediator authenticationMediator)
    {
        _resources = resources;
        _developerIdProvider = developerIdProvider;
        _authenticationMediator = authenticationMediator;
        _authenticationMediator.SignInAction += ResetCommand;
        _authenticationMediator.SignOutAction += ResetCommand;
        Name = _resources.GetResource("Forms_Sign_In");
        Icon = GitHubIcon.IconDictionary["logo"];
    }

    private void ResetCommand(object? sender, SignInStatusChangedEventArgs e)
    {
        _invoked = e.IsSignedIn;
    }

    public override CommandResult Invoke()
    {
        if (_invoked)
        {
            return CommandResult.KeepOpen();
        }

        Task.Run(async () =>
        {
            _invoked = true;
            _authenticationMediator.SetLoadingState(true);
            try
            {
                var signInSucceeded = await _developerIdProvider.LoginNewDeveloperIdAsync();
                _authenticationMediator.SetLoadingState(false);
                _authenticationMediator.SignIn(new SignInStatusChangedEventArgs(true, null));
                ToastHelper.ShowToast(_resources.GetResource("Message_Sign_In_Success"), MessageState.Success);
            }
            catch (Exception ex)
            {
                _authenticationMediator.SetLoadingState(false);
                _authenticationMediator.SignIn(new SignInStatusChangedEventArgs(false, ex));
                ToastHelper.ShowToast($"{_resources.GetResource("Message_Sign_In_Fail")} {ex.Message}", MessageState.Error);
            }
        });
        return CommandResult.KeepOpen();
    }
}
