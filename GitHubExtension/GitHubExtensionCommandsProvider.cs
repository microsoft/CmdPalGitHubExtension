// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls.Forms;
using GitHubExtension.Controls.Pages;
using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension;

public partial class GitHubExtensionCommandsProvider : CommandProvider
{
    private readonly SavedSearchesPage _savedSearchesPage;
    private readonly SignOutPage _signOutPage;
    private readonly SignInPage _signInPage;
    private readonly IDeveloperIdProvider _developerIdProvider;

    public GitHubExtensionCommandsProvider(
        SavedSearchesPage savedSearchesPage,
        SignOutPage signOutPage,
        SignInPage signInPage,
        IDeveloperIdProvider developerIdProvider)
    {
        DisplayName = "GitHub Extension";

        _savedSearchesPage = savedSearchesPage;
        _signOutPage = signOutPage;
        _signInPage = signInPage;
        _developerIdProvider = developerIdProvider;

        // Static events here. Hard dependency. But maybe it is ok in this case
        SignInForm.SignInAction += OnSignInStatusChanged;
        SignOutForm.SignOutAction += OnSignInStatusChanged;

        UpdateSignInStatus(IsSignedIn());
    }

    private void UpdateTopLevelCommands(object? sender, int items) => RaiseItemsChanged(items);

    private bool _isSignedIn;

    public override ICommandItem[] TopLevelCommands()
    {
        return _isSignedIn
        ? [
            new CommandItem(_savedSearchesPage)
            {
                Title = "Saved GitHub Searches",
                Icon = new IconInfo("\ue721"),
            },

            // new CommandItem(new SignOutPage(new SignOutForm(developerIdProvider), new StatusMessage(), "Sign out succeeded!", "Sign out failed"))
            new CommandItem(_signOutPage)
            {
                Title = "GitHub Extension",
                Subtitle = "Sign out",
                Icon = new IconInfo(GitHubIcon.IconDictionary["logo"]),
            },
        ]
        : [

            // new CommandItem(new SignInPage(new SignInForm(developerIdProvider), new StatusMessage(), "Sign in succeeded!", "Sign in failed"))
            new CommandItem(_signInPage)
            {
                Title = "GitHub Extension",
                Subtitle = "Log in",
                Icon = new IconInfo(GitHubIcon.IconDictionary["logo"]),
            }
        ];
    }

    private bool IsSignedIn()
    {
        var devIds = _developerIdProvider.GetLoggedInDeveloperIdsInternal();
        return devIds.Any();
    }

    public void UpdateSignInStatus(bool isSignedIn)
    {
        _isSignedIn = isSignedIn;
        var numCommands = _isSignedIn ? 5 : 2;

        UpdateTopLevelCommands(null, numCommands);
    }

    private void OnSignInStatusChanged(object? sender, SignInStatusChangedEventArgs e)
    {
        UpdateSignInStatus(e.IsSignedIn);
    }
}
