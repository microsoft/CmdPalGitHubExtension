// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Forms;
using GitHubExtension.Helpers;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Windows.Foundation;

namespace GitHubExtension.Pages;

internal sealed partial class GitHubAuthPage : FormPage
{
    private readonly GitHubAuthForm _authForm;

    public override IForm[] Forms() => new IForm[] { _authForm };

    private StatusMessage _authFormStatusMessage = new();

    internal event TypedEventHandler<object, SignInStatusChangedEventArgs>? SignInAction
    {
        add => _authForm.SignInAction += value;
        remove => _authForm.SignInAction -= value;
    }

    public GitHubAuthPage()
    {
        _authForm = new();
        _authForm.SignInAction += OnSignInCompleted;
        _authForm.LoadingStateChanged += OnLoadingChanged;
    }

    private void OnSignInCompleted(object sender, SignInStatusChangedEventArgs args)
    {
        if (args.Error != null)
        {
            IsLoading = false;
            _authFormStatusMessage.Message = $"Error in sign-in: {args.Error.Message}";
            _authFormStatusMessage.State = MessageState.Error;
            ExtensionHost.Host?.ShowStatus(_authFormStatusMessage);
        }
        else if (args.IsSignedIn)
        {
            IsLoading = false;
            _authFormStatusMessage.Message = "Sign in succeeded!";
            _authFormStatusMessage.State = MessageState.Success;
            ExtensionHost.ShowStatus(_authFormStatusMessage);
        }
    }

    private void OnLoadingChanged(object sender, bool isLoading)
    {
        IsLoading = isLoading;
    }
}
