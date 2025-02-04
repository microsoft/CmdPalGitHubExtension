﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Helpers;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace GitHubExtension.Pages;

internal sealed partial class SignOutPage : FormPage
{
    public override IForm[] Forms()
    {
        ExtensionHost.HideStatus(_signOutFormStatusMessage);
        IsLoading = false;
        return new IForm[] { new SignOutForm() };
    }

#pragma warning disable IDE0044 // Add readonly modifier
    private StatusMessage _signOutFormStatusMessage = new();
#pragma warning disable IDE0044 // Add readonly modifier

    public SignOutPage()
    {
        SignOutForm.SignOutAction += OnSignOutCompleted;
        SignOutForm.LoadingStateChanged += OnLoadingChanged;
    }

    private void OnSignOutCompleted(object? sender, SignInStatusChangedEventArgs args)
    {
        if (args.Error != null)
        {
            IsLoading = false;
            _signOutFormStatusMessage.Message = $"Error in sign-out: {args.Error.Message}";
            _signOutFormStatusMessage.State = MessageState.Error;
            ExtensionHost.ShowStatus(_signOutFormStatusMessage);
        }
        else if (!args.IsSignedIn)
        {
            IsLoading = false;
            _signOutFormStatusMessage.Message = "Sign out succeeded!";
            _signOutFormStatusMessage.State = MessageState.Success;
            ExtensionHost.ShowStatus(_signOutFormStatusMessage);
        }
    }

    private void OnLoadingChanged(object sender, bool isLoading)
    {
        IsLoading = isLoading;
    }
}
