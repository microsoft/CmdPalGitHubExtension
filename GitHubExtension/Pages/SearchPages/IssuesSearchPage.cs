// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Commands;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension;

internal sealed partial class IssuesSearchPage(PersistentData.Search search) : SearchPage<DataModel.Issue>(search)
{
    protected override ListItem GetListItem(DataModel.Issue item)
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
        };
    }

    protected async override Task<IEnumerable<DataModel.Issue>> LoadContentData(DataModel.Search dsSearch)
    {
        return await Task.Run(() =>
        {
            var res = new List<DataModel.Issue>();

            if (dsSearch?.Issues != null)
            {
                res.AddRange(dsSearch.Issues);
            }

            return res;
        });
    }
}
