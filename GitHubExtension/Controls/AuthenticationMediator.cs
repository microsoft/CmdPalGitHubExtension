// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Helpers;

namespace GitHubExtension.Controls;

public class AuthenticationMediator
{
    public event EventHandler<SignInStatusChangedEventArgs>? SignInAction;

    public event EventHandler<SignInStatusChangedEventArgs>? SignOutAction;

    public event EventHandler<bool>? LoadingStateChanged;

    public AuthenticationMediator()
    {
    }

    public void SignIn(SignInStatusChangedEventArgs args)
    {
        SignInAction?.Invoke(this, args);
    }

    public void SignOut(SignInStatusChangedEventArgs args)
    {
        SignOutAction?.Invoke(this, args);
    }

    public void SetLoadingState(bool isLoading)
    {
        LoadingStateChanged?.Invoke(this, isLoading);
    }
}
