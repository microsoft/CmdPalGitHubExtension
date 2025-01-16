// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Commands;
using GitHubExtension.Helpers;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace GitHubExtension;

internal sealed partial class SearchPullRequestsPage : ListPage
{
    public SearchPullRequestsPage()
    {
        Icon = new(GitHubIcon.IconDictionary["logo_dark"]);
        Name = "Search GitHub Pull Requests";
        this.ShowDetails = true;
    }

    public override IListItem[] GetItems()
    {
        return [
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
                MoreCommands = [new CommandContextItem(new LinkCommand())],
            },
            new ListItem(new PullRequestMarkdownPage())
            {
                Title = "PullRequest title here",
                Subtitle = "PullRequestNumber",
                Icon = new(GitHubIcon.IconDictionary["pullRequest"]),
                Details = new Details()
                {
                    Title = "PullRequest markdown title",
                    Body = "### PullRequest markdown details",
                },
                MoreCommands = [new CommandContextItem(new LinkCommand())],
                Tags = [new Tag()
                        {
                            Text = "@mentioned-user",
                        },
                        new Tag()
                        {
                            Text = "@review-requested",
                        }
                        ],
            }
        ];
    }
}
