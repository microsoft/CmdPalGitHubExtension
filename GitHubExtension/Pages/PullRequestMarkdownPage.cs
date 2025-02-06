// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Commands;
using GitHubExtension.DataModel.DataObjects;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension;

internal sealed partial class PullRequestMarkdownPage : MarkdownPage
{
    private readonly PullRequest _pullRequest;

    public PullRequestMarkdownPage()
    {
        Icon = new IconInfo(GitHubIcon.IconDictionary["pullRequest"]);
        Name = "View";
        _pullRequest = new PullRequest();
    }

    public PullRequestMarkdownPage(PullRequest pullRequest)
    {
        Icon = new IconInfo(GitHubIcon.IconDictionary["pullRequest"]);
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

    public override string[] Bodies()
    {
        var template = $$"""
        # {{_pullRequest.Title}}
        {{_pullRequest.Body}}
        """;
        return new[] { template };
    }
}
