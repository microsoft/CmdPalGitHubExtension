// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls.Commands;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Pages;

public sealed partial class RepositoriesSearchPage(ISearch search, ICacheDataManager cacheDataManager, IResources resources)
    : SearchPage<IRepository>(search, cacheDataManager, resources)
{
    protected override ListItem GetListItem(IRepository item)
    {
        return new ListItem(new LinkCommand(item, Resources))
        {
            Title = item.FullName,
            Icon = GitHubIcon.IconDictionary["repo"],
            Subtitle = item.Description,
            MoreCommands = new CommandContextItem[]
            {
                new(new CopyCommand(item.HtmlUrl, $"{Resources.GetResource("Commands_CopyURL")}", Resources)),
                new(new CopyCommand(item.CloneUrl, $"{Resources.GetResource("Commands_Copy_CloneUrl")}", Resources)),
            },
        };
    }

    protected override async Task<IEnumerable<IRepository>> LoadContentData()
    {
        return await CacheDataManager.GetRepositories(CurrentSearch);
    }
}
