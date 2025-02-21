// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Forms;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Foundation;

namespace GitHubExtension.Pages;

internal sealed partial class GitHubAuthPage : FormPage
{
    public override IForm[] Forms()
    {
        ExtensionHost.HideStatus(_authFormStatusMessage);
        IsLoading = false;
        return new IForm[] { new GitHubAuthForm() };
    }

#pragma warning disable IDE0044 // Add readonly modifier
    private StatusMessage _authFormStatusMessage = new();
#pragma warning disable IDE0044 // Add readonly modifier

    public GitHubAuthPage()
    {
        GitHubAuthForm.SignInAction += OnSignInCompleted;
    }

    private void OnSignInCompleted(object? sender, SignInStatusChangedEventArgs args)
    {
        if (args.Error != null)
        {
            IsLoading = false;
            _authFormStatusMessage.Message = $"Error in sign-in: {args.Error.Message}";
            _authFormStatusMessage.State = MessageState.Error;
            ExtensionHost.ShowStatus(_authFormStatusMessage);
        }
        else if (args.IsSignedIn)
        {
            IsLoading = false;
            _authFormStatusMessage.Message = "Sign in succeeded!";
            _authFormStatusMessage.State = MessageState.Success;
            ExtensionHost.ShowStatus(_authFormStatusMessage);
        }
    }
}
