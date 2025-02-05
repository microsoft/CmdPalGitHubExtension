// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Forms;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Pages;

internal sealed partial class GitHubPatPage : FormPage
{
    private readonly GitHubPatForm apiForm = new();

    public override IForm[] Forms() => [apiForm];

    public GitHubPatPage()
    {
        Name = "Edit PAT (personal access token)";
        Icon = new IconInfo(GitHubIcon.IconDictionary["logo"]);
    }
}
