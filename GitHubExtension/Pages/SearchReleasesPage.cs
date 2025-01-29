// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHubExtension.Commands;
using GitHubExtension.Helpers;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace GitHubExtension;

internal sealed partial class SearchReleasesPage : ListPage
{
    public SearchReleasesPage()
    {
        Icon = new(GitHubIcon.IconDictionary["release"]);
        Name = "Search GitHub Releases";
        this.ShowDetails = true;
    }

    public override IListItem[] GetItems()
    {
        return [
            new ListItem(new ReleaseMarkdownPage())
            {
                Title = "Release title here",
                Subtitle = "ReleaseNumber",
                Icon = new(GitHubIcon.IconDictionary["release"]),
                Details = new Details()
                {
                    Title = "Release markdown title",
                    Body = "### Release markdown details",
                },
            },
            new ListItem(new ReleaseMarkdownPage())
            {
                Title = "Release title here",
                Subtitle = "ReleaseNumber",
                Icon = new(GitHubIcon.IconDictionary["release"]),
                Details = new Details()
                {
                    Title = "Release markdown title",
                    Body = "### Release markdown details",
                },
            }
        ];
    }
}
