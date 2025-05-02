// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Commands;

public class SignInCommand : InvokableCommand
{
    private readonly IDeveloperIdProvider _developerIdProvider;
    private readonly IResources _resources;
    private readonly AuthenticationMediator _authenticationMediator;

    public SignInCommand(IDeveloperIdProvider developerIdProvider, IResources resources, AuthenticationMediator authenticationMediator)
    {
        _resources = resources;
        _developerIdProvider = developerIdProvider;
        _developerIdProvider.OAuthRedirected += DeveloperIdProvider_OAuthRedirected;
        _authenticationMediator = authenticationMediator;

        Name = _resources.GetResource("Forms_Sign_In");
        Icon = GitHubIcon.IconDictionary["logo"];
    }

    public override CommandResult Invoke()
    {
        Task.Run(() =>
        {
            try
            {
                var signInSucceeded = HandleSignIn().Result;

                // stop loading if loading
                _authenticationMediator.SignIn(new SignInStatusChangedEventArgs(signInSucceeded, null));

                // signal a success event w/ signInSucceeded
            }
            catch (Exception ex)
            {
                // stop loading if loading
                _authenticationMediator.SignIn(new SignInStatusChangedEventArgs(false, ex));

                // signal a failure event with the error
            }
        });
        return CommandResult.KeepOpen();
    }

    private async Task<bool> HandleSignIn()
    {
        var numPreviousDevIds = _developerIdProvider.GetLoggedInDeveloperIdsInternal().Count();

        await _developerIdProvider.LoginNewDeveloperIdAsync();

        var numDevIds = _developerIdProvider.GetLoggedInDeveloperIdsInternal().Count();

        return numDevIds > numPreviousDevIds;
    }

    private void DeveloperIdProvider_OAuthRedirected(object? sender, Exception? e)
    {
        if (e is not null)
        {
            _authenticationMediator.SignIn(new SignInStatusChangedEventArgs(false, e));

            // signal an event
            return;
        }
    }
}
