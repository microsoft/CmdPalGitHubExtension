// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Commands;
using GitHubExtension.Helpers;
using Microsoft.CmdPal.Extensions.Helpers;
using Octokit;

namespace GitHubExtension;

internal sealed partial class IssueMarkdownPage : MarkdownPage
{
    private readonly Issue _issue;

    public IssueMarkdownPage()
    {
        Icon = new(GitHubIcon.IconDictionary["issue"]);
        Name = "View";
        _issue = new Issue();
    }

    public IssueMarkdownPage(Issue issue)
    {
        Icon = new(GitHubIcon.IconDictionary["issue"]);
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

    public override string[] Bodies()
    {
        var template = $$"""
        # {{_issue.Title}}
        ## Respository: {{_issue.Repository}}
        {{_issue.Body}}
        """;
        return [template];
    }
}
