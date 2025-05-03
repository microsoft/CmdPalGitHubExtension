// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Pages;

public class SignInPage : ContentPage
{
    private readonly IDeveloperIdProvider _developerIdProvider;
    private readonly IResources _resources;
    private readonly AuthenticationMediator _authenticationMediator;
    private readonly string _successMessage;
    private readonly string _failureMessage;
    private readonly StatusMessage _statusMessage;

    public SignInPage(IDeveloperIdProvider developerIdProvider, IResources resources, StatusMessage statusMessage, AuthenticationMediator authenticationMediator)
    {
        _resources = resources;
        _developerIdProvider = developerIdProvider;
        _developerIdProvider.OAuthRedirected += DeveloperIdProvider_OAuthRedirected;
        _authenticationMediator = authenticationMediator;
        _successMessage = _resources.GetResource("Message_Sign_In_Success");
        _failureMessage = _resources.GetResource("Message_Sign_In_Fail");
        _statusMessage = statusMessage;

        Name = _resources.GetResource("Forms_Sign_In");
        Icon = GitHubIcon.IconDictionary["logo"];
    }

    public override IContent[] GetContent()
    {
        try
        {
            var signInSucceeded = HandleSignIn().Result;

            _authenticationMediator.SignIn(new SignInStatusChangedEventArgs(signInSucceeded, null));
            EventHelper.RaiseToast(new StatusMessage(), _successMessage, _failureMessage, null, signInSucceeded);
        }
        catch (Exception ex)
        {
            _authenticationMediator.SignIn(new SignInStatusChangedEventArgs(false, ex));

            EventHelper.RaiseToast(new StatusMessage(), _successMessage, _failureMessage, ex, false);
        }

        var form = new FormContent();
        return new IContent[] { form };
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
            return;
        }
    }
}
