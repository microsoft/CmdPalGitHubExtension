// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
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
    private readonly IResources _resources;

    public GitHubExtensionCommandsProvider(
        SavedSearchesPage savedSearchesPage,
        SignOutPage signOutPage,
        SignInPage signInPage,
        IDeveloperIdProvider developerIdProvider,
        ISearchRepository persistentDataManager,
        IResources resources,
        ISearchPageFactory searchPageFactory)
    {
        DisplayName = "GitHub Extension";

        _savedSearchesPage = savedSearchesPage;
        _signOutPage = signOutPage;
        _signInPage = signInPage;
        _developerIdProvider = developerIdProvider;
        _persistentDataManager = persistentDataManager;
        _resources = resources;
        _searchPageFactory = searchPageFactory;

        // Static events here. Hard dependency. But maybe it is ok in this case
        SignInForm.SignInAction += OnSignInStatusChanged;
        SignOutForm.SignOutAction += OnSignInStatusChanged;
        SaveSearchForm.SearchSaved += OnSearchSaved;
        RemoveSavedSearchCommand.SearchRemoved += OnSearchRemoved;

        // This async method raises the RaiseItemsChanged event to update the top-level commands
        // So it is safe if we let it run asynchronously as "fire and forget"
        try
        {
            var task = UpdateSignInStatus(IsSignedIn());
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating sign-in status: {ex.Message}");
        }
    }

    private void OnSearchRemoved(object sender, object args)
    {
        if (args is bool isRemoved && isRemoved)
        {
            RaiseItemsChanged(0);
        }
    }

    private void OnSearchSaved(object? sender, object? args)
    {
        // Calling RaiseItemsChanged whenever a search is saved ensures the
        // top-level commands are updated.
        if (args is SearchCandidate)
        {
            RaiseItemsChanged(0);
        }
    }

    private void UpdateTopLevelCommands() => RaiseItemsChanged(0);

    private bool _isSignedIn;

    public override ICommandItem[] TopLevelCommands()
    {
        Debug.WriteLine($"TopLevelCommands called on thread {Environment.CurrentManagedThreadId}. _isSignedIn: {_isSignedIn}");
        if (!_isSignedIn)
        {
            return new[]
            {
                new CommandItem(_signInPage)
                {
                    Title = "GitHub Extension",
                    Subtitle = _resources.GetResource("Forms_Sign_In"),
                    Icon = new IconInfo(GitHubIcon.IconDictionary["logo"]),
                },
            };
        }

        List<CommandItem> commands;
        commands = GetTopLevelSearchCommands().GetAwaiter().GetResult().ToList();
        Debug.WriteLine($"TopLevelCommands: {string.Join(", ", commands.Select(c => c.Title))}. Thread {Environment.CurrentManagedThreadId}");

        var defaultCommands = new List<CommandItem>
        {
            new(_savedSearchesPage)
            {
                Title = _resources.GetResource("Pages_Saved_Searches"),
                Icon = new IconInfo("\ue721"),
            },
            new(_signOutPage)
            {
                Title = "GitHub Extension",
                Subtitle = _resources.GetResource("Forms_Sign_Out_Button_Title"),
                Icon = new IconInfo(GitHubIcon.IconDictionary["logo"]),
            },
        };

        commands.AddRange(defaultCommands);
        return commands.ToArray();
    }

    private bool IsSignedIn()
    {
        Debug.WriteLine($"Checking sign-in status on thread {Environment.CurrentManagedThreadId}.");
        var devIds = _developerIdProvider.GetLoggedInDeveloperIdsInternal();
        Debug.WriteLine($"Number of developer IDs: {devIds.Count()}. Thread {Environment.CurrentManagedThreadId}");
        return devIds.Any();
    }

    public async Task UpdateSignInStatus(bool isSignedIn)
    {
        Debug.WriteLine($"Updating sign-in status on thread {Environment.CurrentManagedThreadId}.");
        _isSignedIn = isSignedIn;
        var devId = _developerIdProvider.GetLoggedInDeveloperIdsInternal().FirstOrDefault();

        Debug.WriteLine($"Developer ID: {devId?.LoginId}. Thread {Environment.CurrentManagedThreadId}");
        if (_isSignedIn && devId != null)
        {
            try
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

                Debug.WriteLine($"Default searches: {string.Join(", ", defaultSearches.Select(s => s.Name), defaultSearches.Select(t => t.Type))}. Thread {Environment.CurrentManagedThreadId}");

                var defaultTasks = new List<Task>();
                Debug.WriteLine($"Validating default searches on thread {Environment.CurrentManagedThreadId}. defaultSearches: {defaultSearches.ToString()}");
                if (defaultSearches == null || defaultSearches.Count == 0)
                {
                    Debug.WriteLine($"No default searches found on thread {Environment.CurrentManagedThreadId}. Exiting method.");
                    return;
                }

                foreach (var search in defaultSearches)
                {
                    if (search == null)
                    {
                        Debug.WriteLine($"Search is null on thread {Environment.CurrentManagedThreadId}. Skipping.");
                        continue;
                    }

                    var task = Task.Run(async () =>
                    {
                        await _persistentDataManager.ValidateSearch(search);
                        await _persistentDataManager.UpdateSearchTopLevelStatus(search, true);
                    });
                    Debug.WriteLine($"Adding task for search: {search.Name} on thread {Environment.CurrentManagedThreadId}");

                    defaultTasks.Add(task);
                }

                Debug.WriteLine($"Waiting for tasks to complete on thread {Environment.CurrentManagedThreadId}. Task count: {defaultTasks.Count}");
                Debug.WriteLine($"Tasks: {string.Join(", ", defaultTasks.Select(t => t.ToString()))}. Thread {Environment.CurrentManagedThreadId}");
                await Task.WhenAll(defaultTasks);
                UpdateTopLevelCommands();
            }
            catch (Exception ex)
            {
                // Log the exception
                Debug.WriteLine($"Error updating sign-in status on thread {Environment.CurrentManagedThreadId}: {ex.Message}");
            }
        }
        else
        {
            Debug.WriteLine($"User is not signed in. Removing default searches on thread {Environment.CurrentManagedThreadId}");
            UpdateTopLevelCommands();
        }
    }

    private void OnSignInStatusChanged(object? sender, SignInStatusChangedEventArgs e)
    {
        try
        {
            var signInTask = UpdateSignInStatus(e.IsSignedIn);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error handling sign-in status change: {ex.Message}");
        }
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
