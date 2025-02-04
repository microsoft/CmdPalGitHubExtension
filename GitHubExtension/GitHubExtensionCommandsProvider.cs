// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Commands;
using GitHubExtension.DeveloperId;
using GitHubExtension.Forms;
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

        GitHubAuthForm.SignInAction += OnSignInStatusChanged;
        SignOutCommand.SignOutAction += OnSignInStatusChanged;
        TestForm.SignInAction += OnSignInStatusChanged;

        UpdateSignInStatus(IsSignedIn());
    }

    private void UpdateTopLevelCommands(object? sender, int items) => RaiseItemsChanged(items);

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
            new CommandItem(new SignOutCommand())
            {
                Title = "GitHub Extension",
                Subtitle = "Sign out",
                Icon = new(GitHubIcon.IconDictionary["logo"]),
            },
            new CommandItem(new TestPage())
            {
                Title = "Test Page",
                Icon = new(GitHubIcon.IconDictionary["logo"]),
            }
        ]
        : [
            new CommandItem(new GitHubAuthPage())
            {
                Title = "GitHub Extension",
                Subtitle = "Log in",
                Icon = new(GitHubIcon.IconDictionary["logo"]),
            },
            new CommandItem(new TestPage())
            {
                Title = "Test Page",
                Icon = new(GitHubIcon.IconDictionary["logo"]),
            }
        ];
    }

    private static bool IsSignedIn()
    {
        var devIdProvider = DeveloperIdProvider.GetInstance();
        var devIds = devIdProvider.GetLoggedInDeveloperIdsInternal();
        return devIds.Any();
    }

    public void UpdateSignInStatus(bool isSignedIn)
    {
        _isSignedIn = isSignedIn;
        var numCommands = _isSignedIn ? 5 : 2;

        if (_isSignedIn)
        {
            var devIds = DeveloperIdProvider.GetInstance().GetLoggedInDeveloperIdsInternal();
            GitHubRepositoryHelper.Instance.UpdateClient(devIds.First().GitHubClient);
        }
        else
        {
            GitHubRepositoryHelper.Instance.ClearRepositories();
        }

        UpdateTopLevelCommands(null, numCommands);
    }

    private void OnSignInStatusChanged(object? sender, SignInStatusChangedEventArgs e)
    {
        UpdateSignInStatus(e.IsSignedIn);
    }
}
