// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls.Forms;
using GitHubExtension.Controls.Pages.PageTemplates;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Pages;

public sealed partial class SignOutPage : GitHubAuthPage
{
    public SignOutPage(SignOutForm signOutForm, StatusMessage statusMessage, string successMessage, string errorMessage)
        : base(signOutForm, statusMessage, successMessage, errorMessage)
    {
    }
}
