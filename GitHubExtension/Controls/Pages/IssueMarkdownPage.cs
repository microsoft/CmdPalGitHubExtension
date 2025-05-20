// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Controls.Commands;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Pages;

internal sealed partial class IssueContentPage : ContentPage
{
    private readonly IIssue _issue;
    private readonly IResources _resources;

    public IssueContentPage(IIssue issue, IResources resources)
    {
        _resources = resources;
        Icon = GitHubIcon.IconDictionary["issue"];
        Name = _resources.GetResource("Pages_Markdown_Issue");
        _issue = issue;
#pragma warning disable IDE0300 // Simplify collection initialization
        Commands = new CommandContextItem[]
        {
            new(new LinkCommand(issue, resources)),
            new(new CopyCommand(issue.HtmlUrl, _resources.GetResource("Commands_CopyURL"))),
            new(new CopyCommand(issue.Title, _resources.GetResource("Commands_CopyIssueTitle"))),
            new(new CopyCommand(issue.Number.ToString(CultureInfo.InvariantCulture), _resources.GetResource("Commands_CopyIssueNumber"))),
        };
#pragma warning restore IDE0300 // Simplify collection initialization
    }

    public override IContent[] GetContent()
    {
        var template = new MarkdownContent
        {
            Body = $$"""
        # {{_issue.Title}}
        {{_issue.Body}}
        """,
        };

        return [template];
    }
}
