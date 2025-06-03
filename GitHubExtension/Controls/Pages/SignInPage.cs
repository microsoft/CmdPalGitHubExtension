// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls.Commands;
using GitHubExtension.Controls.Forms;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Pages;

public sealed partial class SignInPage : ContentPage, IDisposable
{
    private readonly SignInForm _signInForm;
    private readonly IResources _resources;
    private readonly SignInCommand _signInCommand;
    private readonly AuthenticationMediator _authenticationMediator;

    public SignInPage(SignInForm signInForm, IResources resources, SignInCommand signInCommand, AuthenticationMediator authenticationMediator)
    {
        _signInForm = signInForm;
        _resources = resources;
        Title = _resources.GetResource("ExtensionTitle");
        Name = Title; // Name is for the command, Title is for the page

        // Subtitle in CommandProvider is _resources.GetResource("Forms_Sign_In"),
        Icon = GitHubIcon.IconDictionary["logo"];

        _signInCommand = signInCommand;
        _authenticationMediator = authenticationMediator;
        _authenticationMediator.LoadingStateChanged += OnLoadingStateChanged;

        _signInForm.PropChanged += UpdatePage;

        Commands =
        [
            new CommandContextItem(_signInCommand),
        ];
    }

    private void OnLoadingStateChanged(object? sender, bool isLoading)
    {
        IsLoading = isLoading;
    }

    private void UpdatePage(object sender, IPropChangedEventArgs args)
    {
        RaiseItemsChanged();
    }

    public override IContent[] GetContent()
    {
        return [_signInForm];
    }

    // Disposing area
    private bool _disposed;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _authenticationMediator.LoadingStateChanged -= OnLoadingStateChanged;
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
