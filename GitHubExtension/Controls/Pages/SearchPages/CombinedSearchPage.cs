﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Controls.Commands;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Pages;

public sealed partial class CombinedSearchPage(ISearch search, ICacheDataManager cacheDataManager, IResources resources)
    : SearchPage<IIssue>(search, cacheDataManager, resources)
{
    protected override ListItem GetListItem(IIssue item)
    {
        var iconType = item is IPullRequest ? "pr" : "issue";
        return new ListItem(new LinkCommand(item))
        {
            Title = item.Title,
            Icon = new IconInfo(GitHubIcon.IconDictionary[iconType]),
            Subtitle = $"{GetOwner(item.HtmlUrl)}/{GetRepo(item.HtmlUrl)}/#{item.Number}",
            MoreCommands = item is IPullRequest prItem
                ? new CommandContextItem[]
                {
                    new(new CopyGitCheckoutCommand(prItem, Resources.GetResource("Pages_PullRequest_Checkout"))),
                    new(new CopySourceBranchCommand(prItem, Resources.GetResource("Pages_PullRequest_SourceBranch"))),
                    new(new CopyCommand(prItem.HtmlUrl, Resources.GetResource("Pages_Item_URL"))),
                    new(new CopyCommand(prItem.Title, Resources.GetResource("Pages_PullRequest_Title"))),
                    new(new CopyCommand(prItem.Number.ToString(CultureInfo.InvariantCulture), Resources.GetResource("Pages_PullRequest_Number"))),
                    new(new PullRequestContentPage(prItem)),
                }
                : new CommandContextItem[]
                {
                    new(new CopyCommand(item.HtmlUrl, Resources.GetResource("Pages_Item_URL"))),
                    new(new CopyCommand(item.Title, Resources.GetResource("Pages_Issue_Title"))),
                    new(new CopyCommand(item.Number.ToString(CultureInfo.InvariantCulture), Resources.GetResource("Pages_Issue_Number"))),
                    new(new IssueContentPage(item)),
                },
            Tags = GetTags(item),
        };
    }

    protected async override Task<IEnumerable<IIssue>> LoadContentData()
    {
        var items = await CacheDataManager.GetIssuesAndPullRequests(CurrentSearch);
        if (items == null)
        {
            return Enumerable.Empty<IIssue>();
        }

        return items;
    }
}
