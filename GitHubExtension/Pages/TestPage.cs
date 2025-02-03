// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Forms;
using GitHubExtension.Helpers;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Windows.Foundation;

namespace GitHubExtension.Pages;

internal sealed partial class TestPage : FormPage
{
    private readonly TestForm _testForm;

    public override IForm[] Forms()
    {
        ExtensionHost.HideStatus(_testFormStatusMessage);
        return new IForm[] { _testForm };
    }

#pragma warning disable IDE0044 // Add readonly modifier
    private StatusMessage _testFormStatusMessage = new();
#pragma warning restore IDE0044 // Add readonly modifier

    internal event TypedEventHandler<object, SignInStatusChangedEventArgs>? SignInAction
    {
        add => _testForm.SignInAction += value;
        remove => _testForm.SignInAction -= value;
    }

    public TestPage()
    {
        _testForm = new();
        _testForm.SignInAction += OnSignInCompleted;
        _testForm.LoadingStateChanged += OnLoadingChanged;
    }

    private void OnSignInCompleted(object sender, SignInStatusChangedEventArgs args)
    {
        if (args.Error != null)
        {
            IsLoading = false;
            _testFormStatusMessage.Message = $"Error in sign-in: {args.Error.Message}";
            _testFormStatusMessage.State = MessageState.Error;
            ExtensionHost.Host?.ShowStatus(_testFormStatusMessage);
        }
        else if (args.IsSignedIn)
        {
            IsLoading = false;
            _testFormStatusMessage.Message = "Sign in succeeded!";
            _testFormStatusMessage.State = MessageState.Success;
            ExtensionHost.ShowStatus(_testFormStatusMessage);
        }
    }

    private void OnLoadingChanged(object sender, bool isLoading)
    {
        IsLoading = isLoading;
    }
}
