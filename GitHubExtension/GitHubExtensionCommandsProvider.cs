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
        _savedSearchesPage = savedSearchesPage;
        _signOutPage = signOutPage;
        _signInPage = signInPage;
        _developerIdProvider = developerIdProvider;
        _persistentDataManager = persistentDataManager;
        _resources = resources;
        _searchPageFactory = searchPageFactory;

        DisplayName = _resources.GetResource("ExtensionTitle");

        // Static events here. Hard dependency. But maybe it is ok in this case
        SignInForm.SignInAction += OnSignInStatusChanged;
        SignOutForm.SignOutAction += OnSignInStatusChanged;
        SaveSearchForm.SearchSaved += OnSearchSaved;
        RemoveSavedSearchCommand.SearchRemoved += OnSearchRemoved;

        // This async method raises the RaiseItemsChanged event to update the top-level commands
        // So it is safe if we let it run asynchronously as "fire and forget"
        _ = UpdateSignInStatus(IsSignedIn());
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
        if (!_isSignedIn)
        {
            return new[]
            {
                new CommandItem(_signInPage)
                {
                    Title = _resources.GetResource("ExtensionTitle"),
                    Subtitle = _resources.GetResource("Forms_Sign_In"),
                    Icon = new IconInfo(GitHubIcon.IconDictionary["logo"]),
                },
            };
        }

        List<CommandItem> commands = new();

        // commands = GetTopLevelSearchCommands().GetAwaiter().GetResult().ToList();
        var defaultCommands = new List<CommandItem>
        {
            new(_savedSearchesPage)
            {
                Title = _resources.GetResource("Pages_Saved_Searches"),
                Icon = new IconInfo("\ue721"),
            },
            new(_signOutPage)
            {
                Title = _resources.GetResource("ExtensionTitle"),
                Subtitle = _resources.GetResource("Forms_Sign_Out_Button_Title"),
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

    public async Task UpdateSignInStatus(bool isSignedIn)
    {
        _isSignedIn = isSignedIn;
        var devId = _developerIdProvider.GetLoggedInDeveloperIdsInternal().FirstOrDefault();

        if (_isSignedIn && devId != null)
        {
            var login = devId.LoginId;
            List<ISearch> defaultSearches = new List<ISearch>
            {
                new SearchCandidate($"is:open archived:false assignee:{login} sort:created-desc", _resources.GetResource("CommandsProvider_AssignedToMeCommandName")),
                new SearchCandidate($"is:open is:pr review-requested:{login} archived:false sort:created-desc", _resources.GetResource("CommandsProvider_ReviewRequestedCommandName")),
                new SearchCandidate($"is:open mentions:{login} archived:false sort:created-desc", _resources.GetResource("CommandsProvider_MentionsMeCommandName")),
                new SearchCandidate($"is:open is:issue archived:false author:{login} sort:created-desc", _resources.GetResource("CommandsProvider_CreatedIssuesCommandName")),
                new SearchCandidate($"is:open is:pr author:{login} archived:false sort:created-desc", _resources.GetResource("CommandsProvider_MyPullRequestsCommandName")),
            };

            try
            {
                await _persistentDataManager.InitializeTopLevelSearches(defaultSearches);
            }
            catch (Exception ex)
            {
                var message = $"{_resources.GetResource("CommandsProvider_DefaultCommands_Error")}: {ex.Message}";
                Debug.WriteLine(message);
                if (ex is Octokit.ApiException)
                {
                    Octokit.ApiException apiException = (Octokit.ApiException)ex;
                    message += $" - {StringHelper.ParseHttpErrorMessage(apiException.HttpResponse?.Body?.ToString())}";
                }

                ExtensionHost.LogMessage(new LogMessage() { Message = ex.Message });
                var statusMessage = new StatusMessage
                {
                    Message = message,
                    State = MessageState.Error,
                };
                ExtensionHost.ShowStatus(statusMessage, StatusContext.Page);
            }
        }

        UpdateTopLevelCommands();
    }

    private void OnSignInStatusChanged(object? sender, SignInStatusChangedEventArgs e)
    {
        _ = UpdateSignInStatus(e.IsSignedIn);
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
