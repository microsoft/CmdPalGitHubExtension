// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Controls.Commands;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Pages;

internal sealed partial class PullRequestContentPage : ContentPage
{
    private readonly IPullRequest _pullRequest;

    public PullRequestContentPage(IPullRequest pullRequest)
    {
        Icon = new IconInfo(GitHubIcon.IconDictionary["pr"]);
        Name = "View pull request in Command Palette";
        _pullRequest = pullRequest;
#pragma warning disable IDE0300 // Simplify collection initialization
        Commands = new CommandContextItem[]
        {
            new(new LinkCommand(pullRequest)),
            new(new CopyCommand(pullRequest.HtmlUrl, "URL")),
            new(new CopyCommand(pullRequest.Title, "pull request title")),
            new(new CopyCommand(pullRequest.Number.ToString(CultureInfo.InvariantCulture), "pull request number")),
        };
#pragma warning restore IDE0300 // Simplify collection initialization
    }

    public override IContent[] GetContent()
    {
        var template = new MarkdownContent
        {
            Body = $$"""
                # {{_pullRequest.Title}}
                {{_pullRequest.Body}}
                """,
        };

        return [template];
    }
}
