// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Commands;
using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using GitHubExtension.Pages;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;

namespace GitHubExtension;

public partial class GitHubExtensionActionsProvider : CommandProvider
{
    public GitHubExtensionActionsProvider()
    {
        DisplayName = "GitHub Extension";

        _authPage = new GitHubAuthPage();
        _authPage.SignInAction += UpdateTopLevelCommands;
        _signOutCommand = new SignOutCommand();
        _signOutCommand.SignOutAction += UpdateTopLevelCommands;
    }

    private void UpdateTopLevelCommands(object sender, object? args) => RaiseItemsChanged(0);

    private readonly GitHubAuthPage _authPage;

    private readonly SignOutCommand _signOutCommand;

    public override ICommandItem[] TopLevelCommands()
    {
        return IsSignedIn()
            ? [
            new CommandItem(new SearchIssuesPage())
            {
                Title = "Search GitHub Issues",
                Icon = new(GitHubIcon.IconDictionary["issue"]),
            },
            new CommandItem(new SearchPullRequestsPage())
            {
                Title = "Search GitHub Pull Requests",
                Icon = new(GitHubIcon.IconDictionary["pullRequest"]),
            },
            new CommandItem(_signOutCommand)
            {
                Title = "GitHub Extension",
                Subtitle = "Sign out",
                Icon = new(GitHubIcon.IconDictionary["logo"]),
            },
            ]
            : [new CommandItem(_authPage)
            {
                Title = "GitHub Extension",
                Subtitle = "Log in",
                Icon = new(GitHubIcon.IconDictionary["logo"]),
            },
            ];
    }

    private static bool IsSignedIn()
    {
        var devIdProvider = DeveloperIdProvider.GetInstance();
        var devIds = devIdProvider.GetLoggedInDeveloperIdsInternal();

        return devIds.Any();
    }
}
