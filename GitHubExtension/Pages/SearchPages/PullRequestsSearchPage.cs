// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Commands;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension;

internal sealed partial class PullRequestsSearchPage(PersistentData.Search search) : SearchPage<DataModel.PullRequest>(search)
{
    protected override ListItem GetListItem(DataModel.PullRequest item)
    {
        return new ListItem(new LinkCommand(item))
        {
            Title = item.Title,
            Icon = new IconInfo(GitHubIcon.IconDictionary["pr"]),
            Subtitle = $"{GetOwner(item.HtmlUrl)}/{GetRepo(item.HtmlUrl)}/#{item.Number}",
            MoreCommands = new CommandContextItem[]
            {
                new(new CopyCommand($"git checkout {item.SourceBranch}", "checkout command")),
                new(new CopyCommand(item.SourceBranch, "source branch")),
                new(new CopyCommand(item.HtmlUrl, "URL")),
                new(new CopyCommand(item.Title, "pull request title")),
                new(new CopyCommand(item.Number.ToString(CultureInfo.InvariantCulture), "pull request number")),
                new(new PullRequestContentPage(item)),
            },
        };
    }

    protected async override Task<IEnumerable<DataModel.PullRequest>> LoadContentData(DataModel.Search dsSearch)
    {
        return await Task.Run(() =>
        {
            var res = new List<DataModel.PullRequest>();

            if (dsSearch?.PullRequests != null)
            {
                res.AddRange(dsSearch.PullRequests);
            }

            return res;
        });
    }
}
