// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Commands;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Pages;

internal sealed partial class IssueContentPage : ContentPage
{
    private readonly IIssue _issue;

    public IssueContentPage(IIssue issue)
    {
        Icon = new IconInfo(GitHubIcon.IconDictionary["issue"]);
        Name = "View issue in Command Palette";
        _issue = issue;
#pragma warning disable IDE0300 // Simplify collection initialization
        Commands = new CommandContextItem[]
        {
            new(new LinkCommand(issue)),
            new(new CopyCommand(issue.HtmlUrl, "URL")),
            new(new CopyCommand(issue.Title, "issue title")),
            new(new CopyCommand(issue.Number.ToString(CultureInfo.InvariantCulture), "issue number")),
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
