// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using GitHubExtension.Controls;
using GitHubExtension.Controls.Forms;
using GitHubExtension.Controls.Pages;
using GitHubExtension.DataManager;
using GitHubExtension.DeveloperIds;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension;

public partial class GitHubExtensionCommandsProvider : CommandProvider, IDisposable
{
    private readonly SavedSearchesPage _savedSearchesPage;
    private readonly SignOutPage _signOutPage;
    private readonly SignInPage _signInPage;
    private readonly NotificationsPage _notificationsPage;
    private readonly IDeveloperIdProvider _developerIdProvider;
    private readonly ISearchRepository _persistentDataManager;
    private readonly ISearchPageFactory _searchPageFactory;
    private readonly IResources _resources;
    private readonly SavedSearchesMediator _savedSearchesMediator;
    private readonly AuthenticationMediator _authenticationMediator;
    private readonly NotificationsMediator _notificationsMediator;
    private readonly IGitHubCreateManager _createManager;
    private readonly WorkflowRunsPage _workflowRunsPage;
    private readonly IWorkflowRunsDataManager _workflowRunsDataManager;
    private readonly DiscussionsPage _myDiscussionsPage;
    private readonly DiscussionsPage _searchDiscussionsPage;
    private readonly ProjectsPage _projectsPage;
    private readonly IRepositoryCloneManager _cloneManager;
    private readonly ICloneSettingsStore _cloneSettingsStore;

    public GitHubExtensionCommandsProvider(
        SavedSearchesPage savedSearchesPage,
        SignOutPage signOutPage,
        SignInPage signInPage,
        NotificationsPage notificationsPage,
        IDeveloperIdProvider developerIdProvider,
        ISearchRepository persistentDataManager,
        IResources resources,
        ISearchPageFactory searchPageFactory,
        SavedSearchesMediator savedSearchesMediator,
        AuthenticationMediator authenticationMediator,
        NotificationsMediator notificationsMediator,
        IGitHubCreateManager createManager,
        WorkflowRunsPage workflowRunsPage,
        IWorkflowRunsDataManager workflowRunsDataManager,
        DiscussionsPage myDiscussionsPage,
        DiscussionsPage searchDiscussionsPage,
        ProjectsPage projectsPage,
        IRepositoryCloneManager cloneManager,
        ICloneSettingsStore cloneSettingsStore)
    {
        _savedSearchesPage = savedSearchesPage;
        _signOutPage = signOutPage;
        _signInPage = signInPage;
        _notificationsPage = notificationsPage;
        _developerIdProvider = developerIdProvider;
        _persistentDataManager = persistentDataManager;
        _resources = resources;
        _searchPageFactory = searchPageFactory;
        _savedSearchesMediator = savedSearchesMediator;
        _authenticationMediator = authenticationMediator;
        _notificationsMediator = notificationsMediator;
        _createManager = createManager;
        _workflowRunsPage = workflowRunsPage;
        _workflowRunsDataManager = workflowRunsDataManager;
        _myDiscussionsPage = myDiscussionsPage;
        _searchDiscussionsPage = searchDiscussionsPage;
        _projectsPage = projectsPage;
        _cloneManager = cloneManager;
        _cloneSettingsStore = cloneSettingsStore;

        DisplayName = _resources.GetResource("ExtensionTitle");

        _authenticationMediator.SignInAction += OnSignInStatusChanged;
        _authenticationMediator.SignOutAction += OnSignInStatusChanged;
        _savedSearchesMediator.SearchSaved += OnSearchSaved;
        _savedSearchesMediator.SearchRemoved += OnSearchRemoved;
        _notificationsMediator.NotificationsChanged += OnNotificationsChanged;

        // This async method raises the RaiseItemsChanged event to update the top-level commands
        // So it is safe if we let it run asynchronously as "fire and forget"
        _ = UpdateSignInStatus(_developerIdProvider.IsSignedIn());
    }

    private void OnNotificationsChanged(object? sender, object? args)
    {
        // Refreshes the top-level command subtitle when the unread count changes.
        RaiseItemsChanged(0);
    }

    private void OnSearchRemoved(object? sender, SavedSearchRemovedEventArgs args)
    {
        if (args.RemoveSucceeded)
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
                new CommandItem(_signInPage),
            };
        }

        var commands = GetTopLevelSearchCommands().GetAwaiter().GetResult().ToList();
        var defaultCommands = new List<CommandItem>
        {
            BuildNotificationsCommandItem(),
            BuildCreateIssueCommandItem(),
            BuildCreatePullRequestCommandItem(),
            BuildCreateBranchCommandItem(),
            BuildWorkflowRunsCommandItem(),
            BuildTriggerWorkflowCommandItem(),
            BuildMyDiscussionsCommandItem(),
            BuildSearchDiscussionsCommandItem(),
            BuildProjectsCommandItem(),
            BuildCloneRepositoryCommandItem(),
            new(_savedSearchesPage),
            new(_signOutPage),
        };

        commands.AddRange(defaultCommands);
        return commands.ToArray();
    }

    private CommandItem BuildCreateIssueCommandItem()
    {
        var page = new GitHubFormPage(
            new CreateIssueForm(_createManager, _resources),
            _resources,
            "Forms_CreateIssue_Title",
            "\uE710",
            "Message_CreateIssue_Success",
            "Message_CreateIssue_Error");

        return new CommandItem(page)
        {
            Title = _resources.GetResource("CommandsProvider_CreateIssueCommandName"),
            Subtitle = _resources.GetResource("CommandsProvider_CreateIssueSubtitle"),
        };
    }

    private CommandItem BuildCreatePullRequestCommandItem()
    {
        var page = new GitHubFormPage(
            new CreatePullRequestForm(_createManager, _resources),
            _resources,
            "Forms_CreatePullRequest_Title",
            "\uE710",
            "Message_CreatePullRequest_Success",
            "Message_CreatePullRequest_Error");

        return new CommandItem(page)
        {
            Title = _resources.GetResource("CommandsProvider_CreatePullRequestCommandName"),
            Subtitle = _resources.GetResource("CommandsProvider_CreatePullRequestSubtitle"),
        };
    }

    private CommandItem BuildCreateBranchCommandItem()
    {
        var page = new GitHubFormPage(
            new CreateBranchForm(_createManager, _resources),
            _resources,
            "Forms_CreateBranch_Title",
            "\uE710",
            "Message_CreateBranch_Success",
            "Message_CreateBranch_Error");

        return new CommandItem(page)
        {
            Title = _resources.GetResource("CommandsProvider_CreateBranchCommandName"),
            Subtitle = _resources.GetResource("CommandsProvider_CreateBranchSubtitle"),
        };
    }

    private CommandItem BuildWorkflowRunsCommandItem()
    {
        return new CommandItem(_workflowRunsPage)
        {
            Title = _resources.GetResource("CommandsProvider_WorkflowRunsCommandName"),
            Subtitle = _resources.GetResource("CommandsProvider_WorkflowRunsSubtitle"),
        };
    }

    private CommandItem BuildTriggerWorkflowCommandItem()
    {
        var page = new GitHubFormPage(
            new TriggerWorkflowForm(_workflowRunsDataManager, _resources),
            _resources,
            "Forms_TriggerWorkflow_Title",
            "\uE724",
            "Message_TriggerWorkflow_Success",
            "Message_TriggerWorkflow_Error");

        return new CommandItem(page)
        {
            Title = _resources.GetResource("CommandsProvider_TriggerWorkflowCommandName"),
            Subtitle = _resources.GetResource("CommandsProvider_TriggerWorkflowSubtitle"),
        };
    }

    private CommandItem BuildMyDiscussionsCommandItem()
    {
        return new CommandItem(_myDiscussionsPage)
        {
            Title = _resources.GetResource("CommandsProvider_MyDiscussionsCommandName"),
            Subtitle = _resources.GetResource("CommandsProvider_MyDiscussionsSubtitle"),
        };
    }

    private CommandItem BuildSearchDiscussionsCommandItem()
    {
        return new CommandItem(_searchDiscussionsPage)
        {
            Title = _resources.GetResource("CommandsProvider_SearchDiscussionsCommandName"),
            Subtitle = _resources.GetResource("CommandsProvider_SearchDiscussionsSubtitle"),
        };
    }

    private CommandItem BuildProjectsCommandItem()
    {
        return new CommandItem(_projectsPage)
        {
            Title = _resources.GetResource("CommandsProvider_ProjectsCommandName"),
            Subtitle = _resources.GetResource("CommandsProvider_ProjectsSubtitle"),
        };
    }

    private CommandItem BuildCloneRepositoryCommandItem()
    {
        var page = new GitHubFormPage(
            new CloneRepositoryForm(_cloneManager, _cloneSettingsStore, _resources),
            _resources,
            "Forms_Clone_Title",
            "\uE896",
            "Message_CloneRepository_Success",
            "Message_CloneRepository_Error");

        return new CommandItem(page)
        {
            Title = _resources.GetResource("CommandsProvider_CloneRepositoryCommandName"),
            Subtitle = _resources.GetResource("CommandsProvider_CloneRepositorySubtitle"),
        };
    }

    private CommandItem BuildNotificationsCommandItem()
    {
        var subtitle = _notificationsPage.UnreadCount > 0
            ? string.Format(System.Globalization.CultureInfo.CurrentCulture, _resources.GetResource("CommandsProvider_Notifications_UnreadCount"), _notificationsPage.UnreadCount)
            : _resources.GetResource("CommandsProvider_Notifications_NoUnread");

        return new CommandItem(_notificationsPage)
        {
            Title = _resources.GetResource("CommandsProvider_NotificationsCommandName"),
            Subtitle = subtitle,
        };
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
                new SearchCandidate($"user:{login} sort:updated-desc type:repository", _resources.GetResource("CommandsProvider_MyLatestRepositoriesCommandName")),
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

        if (_isSignedIn)
        {
            // Populate the unread count for the top-level Notifications subtitle
            // without requiring the user to open the page. This refreshes on
            // sign-in (event-driven), not on a polling loop.
            _ = _notificationsPage.RefreshUnreadCountAsync();
        }
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

    // Disposing area
    private bool _disposed;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _authenticationMediator.SignInAction -= OnSignInStatusChanged;
                _authenticationMediator.SignOutAction -= OnSignInStatusChanged;
                _savedSearchesMediator.SearchSaved -= OnSearchSaved;
                _savedSearchesMediator.SearchRemoved -= OnSearchRemoved;
                _notificationsMediator.NotificationsChanged -= OnNotificationsChanged;
            }

            _disposed = true;
        }
    }

    void IDisposable.Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
