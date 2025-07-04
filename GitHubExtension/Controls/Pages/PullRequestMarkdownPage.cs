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
    private readonly IResources _resources;

    public PullRequestContentPage(IPullRequest pullRequest, IResources resources)
    {
        _resources = resources;

        Icon = GitHubIcon.IconDictionary["pr"];
        Name = _resources.GetResource("Pages_Markdown_PullRequest");
        _pullRequest = pullRequest;
#pragma warning disable IDE0300 // Simplify collection initialization
        Commands = new CommandContextItem[]
        {
            new(new LinkCommand(pullRequest, resources)),
            new(new CopyCommand(pullRequest.HtmlUrl, _resources.GetResource("Commands_CopyURL"), _resources)),
            new(new CopyCommand(pullRequest.Title, _resources.GetResource("Commands_CopyPullRequestTitle"), _resources)),
            new(new CopyCommand(pullRequest.Number.ToString(CultureInfo.InvariantCulture), _resources.GetResource("Commands_CopyPullRequestNumber"), _resources)),
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
