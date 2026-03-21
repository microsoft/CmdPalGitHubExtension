// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Controls.Commands;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Pages;

public sealed partial class IssuesSearchPage(ISearch search, ICacheDataManager cacheDataManager, IResources resources)
    : SearchPage<IIssue>(search, cacheDataManager, resources)
{
    protected override ListItem GetListItem(IIssue item)
    {
        return new ListItem(new LinkCommand(item, Resources))
        {
            Title = item.Title,
            Icon = GitHubIcon.IconDictionary["issue"],
            Subtitle = $"{GetOwner(item.HtmlUrl)}/{GetRepo(item.HtmlUrl)}/#{item.Number}",
            MoreCommands = new CommandContextItem[]
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
        return await CacheDataManager.GetIssues(CurrentSearch);
    }
}
