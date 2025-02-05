// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Forms;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Foundation;

namespace GitHubExtension.Pages;

internal sealed partial class TestPage : FormPage
{
    public override IForm[] Forms()
    {
        ExtensionHost.HideStatus(_testMessage);
        IsLoading = false;
        return new IForm[] { new TestForm() };
    }

#pragma warning disable IDE0044 // Add readonly modifier
    private StatusMessage _testMessage = new();
#pragma warning disable IDE0044 // Add readonly modifier

    public TestPage()
    {
        TestForm.SignInAction += OnSignInCompleted;
        TestForm.LoadingStateChanged += OnLoadingChanged;
    }

    private void OnSignInCompleted(object? sender, SignInStatusChangedEventArgs args)
    {
        if (args.Error != null)
        {
            IsLoading = false;
            _testMessage.Message = $"Error in sign-in: {args.Error.Message}";
            _testMessage.State = MessageState.Error;
            ExtensionHost.ShowStatus(_testMessage);
        }
        else if (args.IsSignedIn)
        {
            IsLoading = false;
            _testMessage.Message = "Sign in succeeded!";
            _testMessage.State = MessageState.Success;
            ExtensionHost.ShowStatus(_testMessage);
        }
    }

    private void OnLoadingChanged(object sender, bool isLoading)
    {
        IsLoading = isLoading;
    }

    public StatusMessage GetTestMessage()
    {
        return _testMessage;
    }
}
