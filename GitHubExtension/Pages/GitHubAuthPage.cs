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
            var message = new StatusMessage() { Message = $"Error in sign-in: {args.Error.Message}", State = MessageState.Error };
            ExtensionHost.Host?.ShowStatus(message);
        }
        else if (args.IsSignedIn)
        {
            IsLoading = false;
            var message = new StatusMessage() { Message = "Sign in succeeded!", State = MessageState.Success };
            ExtensionHost.Host?.ShowStatus(message);
        }
    }

    private void OnLoadingChanged(object sender, bool isLoading)
    {
        IsLoading = isLoading;
    }
}
