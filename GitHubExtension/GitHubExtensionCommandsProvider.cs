// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.Controls.Commands;
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
    private readonly ISearchRepository _persistentDataManager;
    private readonly ISearchPageFactory _searchPageFactory;

    public GitHubExtensionCommandsProvider(
        SavedSearchesPage savedSearchesPage,
        SignOutPage signOutPage,
        SignInPage signInPage,
        IDeveloperIdProvider developerIdProvider,
        ISearchRepository persistentDataManager,
        ISearchPageFactory searchPageFactory)
    {
        DisplayName = "GitHub Extension";

        _savedSearchesPage = savedSearchesPage;
        _signOutPage = signOutPage;
        _signInPage = signInPage;
        _developerIdProvider = developerIdProvider;
        _persistentDataManager = persistentDataManager;
        _searchPageFactory = searchPageFactory;

        // Static events here. Hard dependency. But maybe it is ok in this case
        SignInForm.SignInAction += OnSignInStatusChanged;
        SignOutForm.SignOutAction += OnSignInStatusChanged;
        SaveSearchForm.SearchSaved += OnSearchSaved;
        RemoveSavedSearchCommand.SearchRemoved += OnSearchRemoved;

        UpdateSignInStatus(IsSignedIn());
    }

    private void OnSearchRemoved(object sender, object args)
    {
        if (args is bool isRemoved && isRemoved)
        {
            RaiseItemsChanged(0);
        }
    }

    private void OnSearchSaved(object sender, object? args)
    {
        // Calling RaiseItemsChanged whenever a search is saved ensures the
        // top-level commands are updated.
        if (args is SearchCandidate)
        {
            RaiseItemsChanged(0);
        }
    }

    private void UpdateTopLevelCommands(object? sender, int items) => RaiseItemsChanged(items);

    private bool _isSignedIn;

    public override ICommandItem[] TopLevelCommands()
    {
        if (!_isSignedIn)
        {
            return new[]
            {
                new CommandItem(_signInPage)
                {
                    Title = "GitHub Extension",
                    Subtitle = "Log in",
                    Icon = new IconInfo(GitHubIcon.IconDictionary["logo"]),
                },
            };
        }

        List<CommandItem> commands;
        commands = GetTopLevelSearchCommands().GetAwaiter().GetResult().ToList();

        var defaultCommands = new List<CommandItem>
        {
            new(_savedSearchesPage)
            {
                Title = "Saved GitHub Searches",
                Icon = new IconInfo("\ue721"),
            },
            new(_signOutPage)
            {
                Title = "GitHub Extension",
                Subtitle = "Sign out",
                Icon = new IconInfo(GitHubIcon.IconDictionary["logo"]),
            },
        };

        commands.AddRange(defaultCommands);
        return commands.ToArray();
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
        var devId = _developerIdProvider.GetLoggedInDeveloperIdsInternal().FirstOrDefault();

        if (_isSignedIn && devId != null)
        {
            var login = devId.LoginId;
            List<ISearch> defaultSearches = new List<ISearch>
            {
                new SearchCandidate($"state:open assignee:{login} archived:false", "Assigned to Me"),
                new SearchCandidate($"state:open is:pr review-requested:{login} archived:false", "Review Requested"),
                new SearchCandidate($"state:open mentions:{login} archived:false", "Mentions Me"),
                new SearchCandidate($"state:open is:issue author:{login} archived:false", "Created Issues"),
                new SearchCandidate($"state:open is:pr author:{login} archived:false", "My PRs"),
            };

            foreach (var search in defaultSearches)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        // UpdateSearchTopLevelStatus validates search internally
                        await _persistentDataManager.UpdateSearchTopLevelStatus(search, true);
                    }
                    catch (InvalidOperationException)
                    {
                        // User isn't signed in, skip
                    }
                });
            }
        }

        UpdateTopLevelCommands(null, numCommands);
    }

    private void OnSignInStatusChanged(object? sender, SignInStatusChangedEventArgs e)
    {
        UpdateSignInStatus(e.IsSignedIn);
    }

    private async Task<List<CommandItem>> GetTopLevelSearchCommands()
    {
        var topLevelSearches = await _persistentDataManager.GetTopLevelSearches();
        List<CommandItem> topLevelSearchCommands = new List<CommandItem>();
        if (topLevelSearches.Any())
        {
            var topLevelSearchPages = topLevelSearches.Select(savedSearch => _searchPageFactory.CreateItemForSearch(savedSearch)).ToList();

            foreach (var searchPage in topLevelSearchPages)
            {
                topLevelSearchCommands.Add(new CommandItem(searchPage));
            }
        }

        return topLevelSearchCommands;
    }
}
