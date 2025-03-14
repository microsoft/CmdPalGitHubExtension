// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Controls.Commands;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Pages;

public sealed partial class PullRequestsSearchPage(ISearch search, ICacheDataManager cacheDataManager, IResources resources)
    : SearchPage<IPullRequest>(search, cacheDataManager, resources)
{
    protected override ListItem GetListItem(IPullRequest item)
    {
        return new ListItem(new LinkCommand(item, Resources))
        {
            Title = item.Title,
            Icon = new IconInfo(GitHubIcon.IconDictionary["pr"]),
            Subtitle = $"{GetOwner(item.HtmlUrl)}/{GetRepo(item.HtmlUrl)}/#{item.Number}",
            MoreCommands = new CommandContextItem[]
            {
                new(new CopyGitCheckoutCommand(item, Resources.GetResource("Pages_PullRequest_Checkout"))),
                new(new CopySourceBranchCommand(item, Resources.GetResource("Pages_PullRequest_SourceBranch"))),
                new(new CopyCommand(item.HtmlUrl, Resources.GetResource("Pages_Item_URL"))),
                new(new CopyCommand(item.Title, Resources.GetResource("Pages_PullRequest_Title"))),
                new(new CopyCommand(item.Number.ToString(CultureInfo.InvariantCulture), Resources.GetResource("Pages_PullRequest_Number"))),
                new(new PullRequestContentPage(item, Resources)),
            },
            Tags = GetTags(item),
        };
    }

    protected async override Task<IEnumerable<IPullRequest>> LoadContentData()
    {
        return await CacheDataManager.GetPullRequests(CurrentSearch);
    }
}
