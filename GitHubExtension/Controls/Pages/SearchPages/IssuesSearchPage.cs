// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Controls.Commands;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Pages;

internal sealed partial class IssuesSearchPage(ISearch search, ICacheDataManager cacheDataManager)
    : SearchPage<IIssue>(search, cacheDataManager)
{
    protected override ListItem GetListItem(IIssue item)
    {
        return new ListItem(new LinkCommand(item))
        {
            Title = item.Title,
            Icon = new IconInfo(GitHubIcon.IconDictionary["issue"]),
            Subtitle = $"{GetOwner(item.HtmlUrl)}/{GetRepo(item.HtmlUrl)}/#{item.Number}",
            MoreCommands = new CommandContextItem[]
            {
                new(new CopyCommand(item.HtmlUrl, "URL")),
                new(new CopyCommand(item.Title, "item title")),
                new(new CopyCommand(item.Number.ToString(CultureInfo.InvariantCulture), "item number")),
                new(new IssueContentPage(item)),
            },
            Tags = GetTags(item),
        };
    }

    protected async override Task<IEnumerable<IIssue>> LoadContentData()
    {
        return await CacheDataManager.GetIssues(CurrentSearch);
    }
}
