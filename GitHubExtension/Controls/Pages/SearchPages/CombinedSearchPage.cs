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
        return new ListItem(new LinkCommand(item, Resources))
        {
            Title = item.Title,
            Icon = GitHubIcon.IconDictionary[iconType],
            Subtitle = $"{GetOwner(item.HtmlUrl)}/{GetRepo(item.HtmlUrl)}/#{item.Number}",
            MoreCommands = item is IPullRequest prItem
                ? new CommandContextItem[]
                {
                    new(new CopyCommand(string.Format(CultureInfo.CurrentCulture, Resources.GetResource("Commands_Copy_GitCheckoutCommand"), prItem.SourceBranch), Resources.GetResource("Commands_Copy_Checkout"), Resources)),
                    new(new CopyCommand(prItem.SourceBranch, Resources.GetResource("Commands_Copy_Source_Branch"), Resources)),
                    new(new CopyCommand(prItem.HtmlUrl, $"{Resources.GetResource("Commands_CopyURL")}", Resources)),
                    new(new CopyCommand(prItem.Title, $"{Resources.GetResource("Commands_Copy")} {Resources.GetResource("Pages_PullRequest_Title")}", Resources)),
                    new(new CopyCommand(prItem.Number.ToString(CultureInfo.InvariantCulture), $"{Resources.GetResource("Commands_CopyPullRequestNumber")}", Resources)),
                    new(new PullRequestContentPage(prItem, Resources)),
                }
                : new CommandContextItem[]
                {
                    new(new CopyCommand(item.HtmlUrl, $"{Resources.GetResource("Commands_CopyURL")}", Resources)),
                    new(new CopyCommand(item.Title, $"{Resources.GetResource("Commands_CopyIssueTitle")}", Resources)),
                    new(new CopyCommand(item.Number.ToString(CultureInfo.InvariantCulture), $"{Resources.GetResource("Commands_CopyIssueNumber")}", Resources)),
                    new(new IssueContentPage(item, Resources)),
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
