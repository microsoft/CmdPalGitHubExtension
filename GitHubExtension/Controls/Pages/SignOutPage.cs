// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls.Commands;
using GitHubExtension.Controls.Forms;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Pages;

public sealed partial class SignOutPage : ContentPage
{
    private readonly SignOutForm _signOutForm;
    private readonly SignOutCommand _signOutCommand;
    private readonly IResources _resources;
    private readonly AuthenticationMediator _authenticationMediator;

    public SignOutPage(IResources resources, SignOutForm signOutForm, SignOutCommand signOutCommand, AuthenticationMediator authenticationMediator)
    {
        _signOutForm = signOutForm;
        _resources = resources;
        _signOutForm.PropChanged += UpdatePage;
        _signOutCommand = signOutCommand;
        _authenticationMediator = authenticationMediator;
        _authenticationMediator.LoadingStateChanged += OnLoadingStateChanged;
        Title = _resources.GetResource("ExtensionTitle");
        Name = Title; // Name is for the command, Title is for the page

        // Subtitle in CommandProvider is _resources.GetResource("Forms_Sign_Out_Button_Title"),
        Icon = GitHubIcon.IconDictionary["logo"];

        Commands =
        [
            new CommandContextItem(_signOutCommand),
        ];
    }

    private void UpdatePage(object sender, IPropChangedEventArgs args)
    {
        RaiseItemsChanged();
    }

    private void OnLoadingStateChanged(object? sender, bool isLoading)
    {
        IsLoading = isLoading;
    }

    public override IContent[] GetContent()
    {
        return new[] { _signOutForm };
    }
}
