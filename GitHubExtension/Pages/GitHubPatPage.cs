﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Forms;
using GitHubExtension.Helpers;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace GitHubExtension.Pages;

internal sealed partial class GitHubPatPage : FormPage
{
    private readonly GitHubPatForm apiForm = new();

    public override IForm[] Forms() => [apiForm];

    public GitHubPatPage()
    {
        Name = "Edit PAT (personal access token)";
        Icon = new(GitHubIcon.IconDictionary["logo"]);
    }
}
