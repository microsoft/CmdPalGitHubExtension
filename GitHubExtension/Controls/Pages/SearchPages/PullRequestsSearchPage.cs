// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Controls.Commands;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Pages.SearchPages;

internal sealed partial class PullRequestsSearchPage(ISearch search, ICacheDataManager cacheDataManager)
    : SearchPage<IPullRequest>(search, cacheDataManager)
{
    protected override ListItem GetListItem(IPullRequest item)
    {
        return new ListItem(new LinkCommand(item))
        {
            Title = item.Title,
            Icon = new IconInfo(GitHubIcon.IconDictionary["pr"]),
            Subtitle = $"{GetOwner(item.HtmlUrl)}/{GetRepo(item.HtmlUrl)}/#{item.Number}",
            MoreCommands = new CommandContextItem[]
            {
                new(new CopyGitCheckoutCommand(item, "checkout command")),
                new(new CopySourceBranchCommand(item, "source branch")),
                new(new CopyCommand(item.HtmlUrl, "URL")),
                new(new CopyCommand(item.Title, "pull request title")),
                new(new CopyCommand(item.Number.ToString(CultureInfo.InvariantCulture), "pull request number")),
                new(new PullRequestContentPage(item)),
            },
            Tags = GetTags(item),
        };
    }

    protected async override Task<IEnumerable<IPullRequest>> LoadContentData()
    {
        return await CacheDataManager.GetPullRequests(CurrentSearch);
    }
}
