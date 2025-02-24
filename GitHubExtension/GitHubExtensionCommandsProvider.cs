// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Commands;
using GitHubExtension.DeveloperId;
using GitHubExtension.Forms;
using GitHubExtension.Helpers;
using GitHubExtension.Pages;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension;

public partial class GitHubExtensionCommandsProvider : CommandProvider
{
    private readonly ISearchHelper _searchHelper;
    private readonly IPagesFactory _pagesFactory;

    public GitHubExtensionCommandsProvider(ISearchHelper searchHelper, IPagesFactory pagesFactory)
    {
        DisplayName = "GitHub Extension";

        _searchHelper = searchHelper;
        _pagesFactory = pagesFactory;

        GitHubAuthForm.SignInAction += OnSignInStatusChanged;
        SignOutForm.SignOutAction += OnSignInStatusChanged;

        UpdateSignInStatus(IsSignedIn());
    }

    private void UpdateTopLevelCommands(object? sender, int items) => RaiseItemsChanged(items);

    private bool _isSignedIn;

    public override ICommandItem[] TopLevelCommands()
    {
        return _isSignedIn
        ? [
            new CommandItem(new AddRepoPage())
            {
                Title = "Add a repo via URL",
                Icon = new IconInfo(GitHubIcon.IconDictionary["logo"]),
            },
            new CommandItem(new SignOutPage())
            {
                Title = "GitHub Extension",
                Subtitle = "Sign out",
                Icon = new IconInfo(GitHubIcon.IconDictionary["logo"]),
            },
            new CommandItem(_pagesFactory.GetSavedSearchesPage())
            {
                Title = "Saved Searches",
                Icon = new IconInfo("\ue74e"),
            },
            new CommandItem(new SampleGitHubFormPage())
            {
                Title = "Sample Form",
                Icon = new IconInfo("\ue74e"),
            },
        ]
        : [
            new CommandItem(new GitHubAuthPage())
            {
                Title = "GitHub Extension",
                Subtitle = "Log in",
                Icon = new IconInfo(GitHubIcon.IconDictionary["logo"]),
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
            _searchHelper.UpdateClient(devIds.First().GitHubClient);
        }

        UpdateTopLevelCommands(null, numCommands);
    }

    private void OnSignInStatusChanged(object? sender, SignInStatusChangedEventArgs e)
    {
        UpdateSignInStatus(e.IsSignedIn);
    }
}
