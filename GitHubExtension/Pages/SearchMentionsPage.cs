// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Commands;
using GitHubExtension.Helpers;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace GitHubExtension;

internal sealed partial class SearchMentionsPage : ListPage
{
    public SearchMentionsPage()
    {
        Icon = new(GitHubIcon.IconDictionary["issue"]);
        Name = "Search GitHub Issues";
        this.ShowDetails = true;
    }

    public override IListItem[] GetItems()
    {
        return [
            new ListItem(new IssueMarkdownPage())
            {
                Title = "Issue title here",
                Subtitle = "IssueNumber",
                Icon = new(GitHubIcon.IconDictionary["issue"]),
                Details = new Details()
                {
                    Title = "Issue markdown title",
                    Body = "### Issue markdown details",
                },
            },
            new ListItem(new PullRequestMarkdownPage())
            {
                Title = "Pull Request title here",
                Subtitle = "Pull RequestNumber",
                Icon = new(GitHubIcon.IconDictionary["pullRequest"]),
                Details = new Details()
                {
                    Title = "Pull Request markdown title",
                    Body = "### Pull Request markdown details",
                },
            }
        ];
    }
}
