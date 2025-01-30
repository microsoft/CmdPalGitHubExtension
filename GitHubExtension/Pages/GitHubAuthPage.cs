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
    private readonly GitHubAuthForm _authForm = new();

    public override IForm[] Forms() => [_authForm];

    internal event TypedEventHandler<object, SignInStatusChangedEventArgs>? SignInAction
    {
        add => _authForm.SignInAction += value;
        remove => _authForm.SignInAction -= value;
    }
}
