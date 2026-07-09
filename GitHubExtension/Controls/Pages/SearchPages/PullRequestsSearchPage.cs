// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Controls.Commands;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Pages;

public sealed partial class PullRequestsSearchPage : SearchPage<IPullRequest>
{
    private readonly MutationCommandsFactory _mutationCommandsFactory;

    public PullRequestsSearchPage(ISearch search, ICacheDataManager cacheDataManager, IResources resources, MutationCommandsFactory mutationCommandsFactory, MutationMediator mutationMediator)
        : base(search, cacheDataManager, resources)
    {
        _mutationCommandsFactory = mutationCommandsFactory;
        mutationMediator.MutationCompleted += OnMutationCompleted;
    }

    private void OnMutationCompleted(object? sender, EventArgs e) => RaiseItemsChanged(0);

    protected override ListItem GetListItem(IPullRequest item)
    {
        var moreCommands = new List<CommandContextItem>
        {
            new(new CopyCommand(string.Format(CultureInfo.CurrentCulture, Resources.GetResource("Commands_Copy_GitCheckoutCommand"), item.SourceBranch), Resources.GetResource("Commands_Copy_Checkout"), Resources)),
            new(new CopyCommand(item.SourceBranch, Resources.GetResource("Commands_Copy_Source_Branch"), Resources)),
            new(new CopyCommand(item.HtmlUrl, Resources.GetResource("Commands_CopyURL"), Resources)),
            new(new CopyCommand(item.Title, Resources.GetResource("Commands_CopyPullRequestTitle"), Resources)),
            new(new CopyCommand(item.Number.ToString(CultureInfo.InvariantCulture), Resources.GetResource("Commands_CopyPullRequestNumber"), Resources)),
            new(new PullRequestContentPage(item, Resources, _mutationCommandsFactory)),
        };
        moreCommands.AddRange(_mutationCommandsFactory.GetPullRequestCommands(item));

        return new ListItem(new LinkCommand(item, Resources))
        {
            Title = item.Title,
            Icon = GitHubIcon.IconDictionary["pr"],
            Subtitle = $"{GetOwner(item.HtmlUrl)}/{GetRepo(item.HtmlUrl)}/#{item.Number}",
            MoreCommands = moreCommands.ToArray(),
            Tags = GetTags(item),
        };
    }

    protected async override Task<IEnumerable<IPullRequest>> LoadContentData()
    {
        return await CacheDataManager.GetPullRequests(CurrentSearch);
    }
}
