// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls.Commands;
using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Pages;

public class AltSignOutPage : ContentPage
{
    private readonly IDeveloperIdProvider _developerIdProvider;
    private readonly IResources _resources;
    private readonly AuthenticationMediator _authenticationMediator;
    private readonly string _successMessage;
    private readonly string _failureMessage;
    private readonly StatusMessage _statusMessage;
    private readonly ConfirmationArgs _signOutConfirmationArgs;

    public AltSignOutPage(IDeveloperIdProvider developerIdProvider, IResources resources, StatusMessage statusMessage, AuthenticationMediator authenticationMediator, SignOutCommand signOutCommand)
    {
        _resources = resources;
        _developerIdProvider = developerIdProvider;
        _authenticationMediator = authenticationMediator;
        _successMessage = _resources.GetResource("Message_Sign_In_Success");
        _failureMessage = _resources.GetResource("Message_Sign_In_Fail");
        _statusMessage = statusMessage;

        _signOutConfirmationArgs = new()
        {
            PrimaryCommand = new SignOutCommand(_developerIdProvider, _resources, new StatusMessage(), _authenticationMediator),
            Title = "Sign out of the GitHub Extension",
            Description = "Are you sure you want to sign out?",
        };
        Name = _resources.GetResource("Forms_Sign_In");
        Icon = GitHubIcon.IconDictionary["logo"];
    }

    public override IContent[] GetContent()
    {
        ConfirmationDialog();
        var form = new FormContent();
        return new IContent[] { form };
    }

    private CommandResult ConfirmationDialog()
    {
        return CommandResult.Confirm(_signOutConfirmationArgs);
    }
}
