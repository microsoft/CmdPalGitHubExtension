﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Client;
using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Octokit;

namespace GitHubExtension;

internal sealed partial class SearchRepositoriesPage : ListPage
{
    private readonly GitHubClient _client;

    public SearchRepositoriesPage()
    {
        Icon = new IconInfo(GitHubIcon.IconDictionary["issue"]);
        Name = "Search GitHub Repositories";
        this.ShowDetails = true;

        _client = GetClient();
    }

    private GitHubClient GetClient()
    {
        var devIdProvider = DeveloperIdProvider.GetInstance();
        var devIds = devIdProvider.GetLoggedInDeveloperIdsInternal();

        var client = devIds.Any() ? devIds.First().GitHubClient : GitHubClientProvider.Instance.GetClient();
        return client;
    }

    public override IListItem[] GetItems() => DoGetItems(SearchText).GetAwaiter().GetResult();

    private async Task<IListItem[]> DoGetItems(string query)
    {
        var repos = await _client.Repository.GetAllForCurrent();
        var user = await _client.User.Current();

        if (repos.Count > 0)
        {
            var section = repos.Select(repo => new ListItem(new IssueMarkdownPage())
            {
                Title = repo.FullName,
                Subtitle = repo.Description,
                Icon = new IconInfo(GitHubIcon.IconDictionary["issue"]),
            }).ToArray();
            return section;
        }
        else
        {
            return repos.Count < 0
                ? [new ListItem(new NoOpCommand()) { Title = "Something went wrong. Count < 0" },
            new ListItem(new NoOpCommand()) { Title = $"User: {user.Name}" },
            new ListItem(new NoOpCommand()) { Title = $"Query: {query}" },]
                : [new ListItem(new NoOpCommand()) { Title = "No issues found" },
            new ListItem(new NoOpCommand()) { Title = $"User: {user.Name}" },
            new ListItem(new NoOpCommand()) { Title = $"Query: {query}" },];
        }
    }
}
