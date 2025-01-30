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
        _authPage.SignInAction += OnSignInStatusChanged;
        _signOutCommand = new SignOutCommand();
        _signOutCommand.SignOutAction += OnSignInStatusChanged;

        _isSignedIn = IsSignedIn();
    }

    private void UpdateTopLevelCommands(object? sender, object? args) => RaiseItemsChanged(0);

    private readonly GitHubAuthPage _authPage;

    private readonly SignOutCommand _signOutCommand;

    private bool _isSignedIn;

    public override ICommandItem[] TopLevelCommands()
    {
        return _isSignedIn
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
            new CommandItem(new AddRepoPage())
            {
                Title = "Add a repo via URL",
                Icon = new(GitHubIcon.IconDictionary["logo"]),
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

    private void OnSignInStatusChanged(object? sender, SignInStatusChangedEventArgs e)
    {
        if (e.IsSignedIn || IsSignedIn())
        {
            var devIds = DeveloperIdProvider.GetInstance().GetLoggedInDeveloperIdsInternal();
            GitHubRepositoryHelper.Instance.UpdateClient(devIds.First().GitHubClient);
            _isSignedIn = true;
        }
        else
        {
            GitHubRepositoryHelper.Instance.ClearRepositories();
            _isSignedIn = false;
        }

        UpdateTopLevelCommands(sender, e);
    }
}
