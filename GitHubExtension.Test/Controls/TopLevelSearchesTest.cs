// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Nodes;
using GitHubExtension.Controls;
using GitHubExtension.Controls.Commands;
using GitHubExtension.Controls.Forms;
using GitHubExtension.Controls.Pages;
using GitHubExtension.DataModel;
using GitHubExtension.DeveloperIds;
using GitHubExtension.Helpers;
using GitHubExtension.PersistentData;
using GitHubExtension.Test.PersistentData;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Resources;
using Moq;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class TopLevelSearchesTest
{
    [TestMethod]
    public async Task SaveSearchForm_KeepsIsTopLevelCheckedIfSearchSavedToTopLevel()
    {
        // Initialize
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions(); // created here because we dispose dataStoreOptions at the end of this test
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);

        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create a top-level command and save it via SaveSearchForm
        var testSearchString = "is:issue author:testuser";
        var testSearchName = "Test Search";
        var jsonPayload = CreateJsonPayload(testSearchString, testSearchName, true);

        saveSearchForm.SubmitForm(jsonPayload, string.Empty);

        await Task.Delay(10);

        // Assert that the search is saved and is top level in the PersistentDataManager
        var savedSearches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(savedSearches.Count() == 1, "Should have only our saved search");
        Assert.IsTrue(savedSearches.Any(s => string.Equals(s.Name, testSearchName, StringComparison.Ordinal) && string.Equals(s.SearchString, s.SearchString, StringComparison.Ordinal)), "The new search should appear in saved searches");

        // Simulate creating a SaveSearchForm for the EditSearchPage. Verify that the IsTopLevel box is checked by checking the GetIsTopLevel on SaveSearchForm.
        var editSearchForm = new SaveSearchForm(savedSearches.First(), persistentDataManager, mockResources, savedSearchesMediator);
        Assert.IsTrue(await editSearchForm.GetIsTopLevel());

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task SaveSearchForm_KeepsIsTopLevelUncheckedIfSearchIsNoLongerSavedToTopLevel()
    {
        // Initialize
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        var stubValidator = new Mock<IGitHubValidator>().Object;
        using var persistentDataManager = new PersistentDataManager(stubValidator, dataStoreOptions);
        var savedSearchesMediator = new SavedSearchesMediator();

        // Create a top-level search and verify that it's listed as top-level
        var dummySearch = new SearchCandidate("dummy search", "Dummy Search", true);

        await persistentDataManager.UpdateSearchTopLevelStatus(dummySearch, true);
        savedSearchesMediator.AddSearch(dummySearch);

        var mockResources = new Mock<IResources>().Object;
        var saveSearchForm = new SaveSearchForm(dummySearch, persistentDataManager, mockResources, savedSearchesMediator);

        var initialTopLevelSearches = await persistentDataManager.GetTopLevelSearches();
        Assert.IsTrue(initialTopLevelSearches.Count() == 1, "Should have only our saved search");
        Assert.IsTrue(initialTopLevelSearches.Any(s => string.Equals(s.Name, dummySearch.Name, StringComparison.Ordinal)
            && string.Equals(s.SearchString, dummySearch.SearchString, StringComparison.Ordinal)));
        Assert.IsTrue(saveSearchForm.GetIsTopLevel().Result);

        // Uncheck the "IsTopLevel" checkbox and verify that the search is no longer top-level
        var jsonPayload = CreateJsonPayload(dummySearch.SearchString, dummySearch.Name, false);
        saveSearchForm.SubmitForm(jsonPayload, string.Empty);

        await Task.Delay(10);

        var editSearchForm = new SaveSearchForm(initialTopLevelSearches.First(), persistentDataManager, mockResources, savedSearchesMediator);
        Assert.IsFalse(await editSearchForm.GetIsTopLevel());

        var updatedTopLevelSearches = await persistentDataManager.GetTopLevelSearches();
        Assert.IsFalse(updatedTopLevelSearches.Any(), "Should have no top-level searches");

        var isTopLevel = await persistentDataManager.IsTopLevel(dummySearch);
        Assert.IsFalse(isTopLevel);
        Assert.IsFalse(saveSearchForm.GetIsTopLevel().Result);

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task Integration_AddNewTopLevelCommand()
    {
        // Initialize
        var mockDeveloperIdProvider = CreateMockDeveloperIdProvider();
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();

        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions(); // created here because we dispose dataStoreOptions at the end of this test
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);

        var mockCacheDataManager = new Mock<ICacheDataManager>().Object;
        var searchPageFactory = new SearchPageFactory(mockCacheDataManager, persistentDataManager, mockResources, savedSearchesMediator);

        var addSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        var mockAddSearchListItem = CreateMockAddSearchListItem();
        var savedSearchesPage = new SavedSearchesPage(searchPageFactory, persistentDataManager, mockResources, mockAddSearchListItem, savedSearchesMediator);

        var commandsProvider = CreateGitHubExtensionCommandsProvider(mockDeveloperIdProvider, mockResources, savedSearchesPage, persistentDataManager, savedSearchesMediator, searchPageFactory);

        // Create a new top level search via the SaveSearchForm
        var testSearchString = "is:issue author:testuser";
        var testSearchName = "New Top Level Search";
        var jsonPayload = CreateJsonPayload(testSearchString, testSearchName, true);
        addSearchForm.SubmitForm(jsonPayload, string.Empty);

        await Task.Delay(10);

        // Assert that search is saved and is top level in the persistentDataManager
        var savedSearches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(
            savedSearches.Any(s =>
            string.Equals(s.Name, testSearchName, StringComparison.Ordinal) &&
            string.Equals(s.SearchString, testSearchString, StringComparison.Ordinal)),
            "The new search should appear in saved searches");

        var persitentDataManagerTopLevelCommands = await persistentDataManager.GetTopLevelSearches();
        Assert.IsTrue(
            persitentDataManagerTopLevelCommands.Any(s =>
            string.Equals(s.Name, testSearchName, StringComparison.Ordinal) &&
            string.Equals(s.SearchString, testSearchString, StringComparison.Ordinal)),
            "The new search should appear in top level commands");

        // Assert that search is displayed in the saved searches page
        var savedSearchesItems = savedSearchesPage.GetItems();
        Assert.IsTrue(savedSearchesItems.Length == 2, "Should have our saved search and the add item");
        Assert.IsTrue(savedSearchesItems.Any(item => string.Equals(item.Title, testSearchName, StringComparison.Ordinal)));

        // Assert that search is in the CommandsProvider's top level commands
        var topLevelCommands = commandsProvider.TopLevelCommands();
        Assert.IsTrue(topLevelCommands.Any(c => string.Equals(c.Title, testSearchName, StringComparison.Ordinal)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    // This test creates a non-top-level search and then edits it to be top-level.
    [TestMethod]
    public async Task Integration_AddAndEditSearch_ToBeTopLevel()
    {
        // Initialize
        var mockDeveloperIdProvider = CreateMockDeveloperIdProvider();
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();

        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions(); // created here because we dispose dataStoreOptions at the end of this test
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);

        var mockCacheDataManager = new Mock<ICacheDataManager>().Object;
        var searchPageFactory = new SearchPageFactory(mockCacheDataManager, persistentDataManager, mockResources, savedSearchesMediator);

        var addSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        var mockAddSearchListItem = CreateMockAddSearchListItem();
        var savedSearchesPage = new SavedSearchesPage(searchPageFactory, persistentDataManager, mockResources, mockAddSearchListItem, savedSearchesMediator);

        var commandsProvider = CreateGitHubExtensionCommandsProvider(mockDeveloperIdProvider, mockResources, savedSearchesPage, persistentDataManager, savedSearchesMediator, searchPageFactory);

        // Create a new non-top-level search via the SaveSearchForm
        var testSearchString = "is:issue author:testuser";
        var testSearchName = "My regular search";
        var jsonPayload = CreateJsonPayload(testSearchString, testSearchName, false);
        addSearchForm.SubmitForm(jsonPayload, string.Empty);

        await Task.Delay(10);

        // Assert saved search is in PersistentDataManager's saved searches, but not top level commands
        var savedSearches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(savedSearches.Count() == 1, "Should have only our saved search");
        Assert.IsTrue(
            savedSearches.Any(s =>
                string.Equals(s.Name, testSearchName, StringComparison.Ordinal) &&
                string.Equals(s.SearchString, testSearchString, StringComparison.Ordinal)),
            "The new search should appear in saved searches");
        var savedSearch = savedSearches.First();
        Assert.IsFalse(
            await persistentDataManager.IsTopLevel(savedSearch));

        // Assert saved search is in SavedSearchesPage
        var savedItems = savedSearchesPage.GetItems();
        Assert.IsTrue(savedItems.Length == 2, "Should have our saved search and the add item");
        Assert.IsTrue(savedItems.Any(item => string.Equals(item.Title, testSearchName, StringComparison.Ordinal)));
        Assert.IsTrue(savedItems.Any(item => string.Equals(item.Title, "Add Saved Search", StringComparison.Ordinal)));

        // Edit saved search to be on the top level
        var editSearchForm = new SaveSearchForm(savedSearch, persistentDataManager, mockResources, savedSearchesMediator);
        var editSearchString = "is:issue author:testuser";
        var editSearchName = "My Regular Search - Now top level";
        var editJsonPayload = CreateJsonPayload(editSearchString, editSearchName, true);

        editSearchForm.SubmitForm(editJsonPayload, string.Empty);

        await Task.Delay(10);

        // Assert the saved search is updated as top level in the PersistentDataManager
        var updatedSavedSearches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(updatedSavedSearches.Count() == 1, "Should have only our saved search");
        Assert.IsTrue(
            updatedSavedSearches.Any(s =>
                string.Equals(s.Name, editSearchName, StringComparison.Ordinal) &&
                string.Equals(s.SearchString, editSearchString, StringComparison.Ordinal)),
            "The edited search should be saved");

        var topLevelCommands = await persistentDataManager.GetTopLevelSearches();
        Assert.IsTrue(
            topLevelCommands.Any(s =>
                string.Equals(s.Name, editSearchName, StringComparison.Ordinal) &&
                string.Equals(s.SearchString, editSearchString, StringComparison.Ordinal)),
            "The search should now appear in top level commands after editing");

        // Assert the saved search now appears in the top level commands
        var topLevelCommandsInCommandsProvider = commandsProvider.TopLevelCommands();
        Assert.IsTrue(topLevelCommandsInCommandsProvider.Any(c => string.Equals(c.Title, editSearchName, StringComparison.Ordinal)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task Integration_RemoveTopLevelCommand_FromSavedSearches()
    {
        // Initialize
        var mockDeveloperIdProvider = CreateMockDeveloperIdProvider();
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();

        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions(); // created here because we dispose dataStoreOptions at the end of this test
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);

        var mockCacheDataManager = new Mock<ICacheDataManager>().Object;
        var searchPageFactory = new SearchPageFactory(mockCacheDataManager, persistentDataManager, mockResources, savedSearchesMediator);

        var addSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        var mockAddSearchListItem = CreateMockAddSearchListItem();
        var savedSearchesPage = new SavedSearchesPage(searchPageFactory, persistentDataManager, mockResources, mockAddSearchListItem, savedSearchesMediator);

        var commandsProvider = CreateGitHubExtensionCommandsProvider(mockDeveloperIdProvider, mockResources, savedSearchesPage, persistentDataManager, savedSearchesMediator, searchPageFactory);

        // Create a new top level search
        // In previous tests, we tested the adding and editing flow. Now, we're testing removing an existing command, so the command is added directly.
        var testSearchString = "is:issue author:testuser";
        var testSearchName = "Top level search";
        var topLevelSearch = new SearchCandidate(testSearchString, testSearchName, true);
        await persistentDataManager.UpdateSearchTopLevelStatus(topLevelSearch, true);
        savedSearchesMediator.AddSearch(topLevelSearch);

        await Task.Delay(10);

        // Ensure the test conditions are set up correctly:
        // Only one saved search and it's the one we added
        var initialSavedSearches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(initialSavedSearches.Count() == 1, "Should have only our saved search");
        Assert.IsTrue(
            initialSavedSearches.Any(s =>
                string.Equals(s.Name, testSearchName, StringComparison.Ordinal) &&
                string.Equals(s.SearchString, testSearchString, StringComparison.Ordinal)),
            "Search should be in saved searches initially");

        // Only one top level search and it's the one we added
        var initialTopLevelSearches = await persistentDataManager.GetTopLevelSearches();
        Assert.IsTrue(initialSavedSearches.Count() == 1, "Should have only our saved search");
        Assert.IsTrue(
            initialTopLevelSearches.Any(s =>
                string.Equals(s.Name, testSearchName, StringComparison.Ordinal) &&
                string.Equals(s.SearchString, testSearchString, StringComparison.Ordinal)),
            "Search should be in top level searches initially");

        // Only one saved search on the SavedSearchesPage and the add search item
        var savedSearchesItems = savedSearchesPage.GetItems();
        Assert.IsTrue(savedSearchesItems.Length == 2, "Should have our saved search and the add item");
        Assert.IsTrue(savedSearchesItems.Any(item => string.Equals(item.Title, testSearchName, StringComparison.Ordinal)));
        Assert.IsTrue(savedSearchesItems.Any(item => string.Equals(item.Title, "Add Saved Search", StringComparison.Ordinal)));

        // The saved search is on the top level
        var topLevelCommands = commandsProvider.TopLevelCommands();
        Assert.IsTrue(topLevelCommands.Any(c => string.Equals(c.Title, testSearchName, StringComparison.Ordinal)));

        // Remove the top level search
        var removeCommand = new RemoveSavedSearchCommand(topLevelSearch, persistentDataManager, mockResources, savedSearchesMediator);
        removeCommand.Invoke();

        await Task.Delay(10);

        // Assert the search is removed from the PersistentDataManager's saved searches and top level searches
        var updatedSavedSearches = await persistentDataManager.GetSavedSearches();
        Assert.IsFalse(
            updatedSavedSearches.Any(s =>
                string.Equals(s.Name, testSearchName, StringComparison.Ordinal) &&
                string.Equals(s.SearchString, testSearchString, StringComparison.Ordinal)),
            "Search should be removed from saved searches");

        var updatedTopLevelSearches = await persistentDataManager.GetTopLevelSearches();
        Assert.IsFalse(
            updatedTopLevelSearches.Any(s =>
                string.Equals(s.Name, testSearchName, StringComparison.Ordinal) &&
                string.Equals(s.SearchString, testSearchString, StringComparison.Ordinal)),
            "Search should be removed from top level searches");

        // Assert the search is removed from the SavedSearchesPage
        var updatedSavedSearchesItems = savedSearchesPage.GetItems();
        Assert.IsTrue(updatedSavedSearchesItems.Length == 1, "Should only have the add item");
        Assert.IsFalse(updatedSavedSearchesItems.Any(item => string.Equals(item.Title, testSearchName, StringComparison.Ordinal)));

        // Assert the search is removed from the top level commands in the CommandsProvider
        var updatedTopLevelCommands = commandsProvider.TopLevelCommands();
        Assert.IsFalse(updatedTopLevelCommands.Any(c => string.Equals(c.Title, testSearchName, StringComparison.Ordinal)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task Integration_EditTopLevelSearch_ToBeNonTopLevel()
    {
        // Initialize
        var mockDeveloperIdProvider = CreateMockDeveloperIdProvider();
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();

        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions(); // created here because we dispose dataStoreOptions at the end of this test
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);

        var mockCacheDataManager = new Mock<ICacheDataManager>().Object;
        var searchPageFactory = new SearchPageFactory(mockCacheDataManager, persistentDataManager, mockResources, savedSearchesMediator);

        var addSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        var mockAddSearchListItem = CreateMockAddSearchListItem();
        var savedSearchesPage = new SavedSearchesPage(searchPageFactory, persistentDataManager, mockResources, mockAddSearchListItem, savedSearchesMediator);

        var commandsProvider = CreateGitHubExtensionCommandsProvider(mockDeveloperIdProvider, mockResources, savedSearchesPage, persistentDataManager, savedSearchesMediator, searchPageFactory);

        // Create a new top-level search and add it directly
        var testSearchString = "is:issue author:testuser";
        var testSearchName = "Top level search";
        var testSearch = new SearchCandidate(testSearchString, testSearchName, true);
        await persistentDataManager.UpdateSearchTopLevelStatus(testSearch, testSearch.IsTopLevel);
        savedSearchesMediator.AddSearch(testSearch);

        await Task.Delay(10);

        // Ensure the test conditions are set up correctly:
        var initialSavedSearches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(initialSavedSearches.Count() == 1, "Should have only our saved search");
        Assert.IsTrue(
            initialSavedSearches.Any(s =>
                string.Equals(s.Name, testSearchName, StringComparison.Ordinal) &&
                string.Equals(s.SearchString, testSearchString, StringComparison.Ordinal)),
            "Search should be in saved searches initially");

        var initialTopLevelSearches = await persistentDataManager.GetTopLevelSearches();
        Assert.IsTrue(initialTopLevelSearches.Count() == 1, "Should have only our saved search");
        Assert.IsTrue(
            initialTopLevelSearches.Any(s =>
                string.Equals(s.Name, testSearchName, StringComparison.Ordinal) &&
                string.Equals(s.SearchString, testSearch.SearchString, StringComparison.Ordinal)),
            "Search should be in top level searches initially");

        var savedSearchesItems = savedSearchesPage.GetItems();
        Assert.IsTrue(savedSearchesItems.Length == 2, "Should have our saved search and the add item");
        Assert.IsTrue(savedSearchesItems.Any(item => string.Equals(item.Title, testSearchName, StringComparison.Ordinal)));
        Assert.IsTrue(savedSearchesItems.Any(item => string.Equals(item.Title, "Add Saved Search", StringComparison.Ordinal)));

        var topLevelCommands = commandsProvider.TopLevelCommands();
        Assert.IsTrue(topLevelCommands.Any(c => string.Equals(c.Title, testSearchName, StringComparison.Ordinal)));

        // Edit the saved search to be non-top-level
        var editSearchForm = new SaveSearchForm(testSearch, persistentDataManager, mockResources, savedSearchesMediator);

        var editJsonPayload = CreateJsonPayload(testSearchString, testSearchName, false);

        editSearchForm.SubmitForm(editJsonPayload, string.Empty);

        await Task.Delay(10);

        // Assert the saved search is updated as non-top level in the PersistentDataManager
        var updatedSavedSearches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(
            updatedSavedSearches.Any(s =>
                string.Equals(s.Name, testSearch.Name, StringComparison.Ordinal) &&
                string.Equals(s.SearchString, testSearch.SearchString, StringComparison.Ordinal)),
            "The search should still appear in saved searches after editing");

        var updatedTopLevelSearches = await persistentDataManager.GetTopLevelSearches();
        Assert.IsFalse(
            updatedTopLevelSearches.Any(s =>
                string.Equals(s.Name, testSearchName, StringComparison.Ordinal) &&
                string.Equals(s.SearchString, testSearch.SearchString, StringComparison.Ordinal)),
            "The search should not appear in top level commands after editing");

        // Assert the search is still in the SavedSearchesPage
        var updatedSavedSearchPageItems = savedSearchesPage.GetItems();
        Assert.IsTrue(updatedSavedSearchPageItems.Length == 2, "Should have our saved search and the add item");
        Assert.IsTrue(updatedSavedSearchPageItems.Any(item => string.Equals(item.Title, testSearchName, StringComparison.Ordinal)));
        Assert.IsTrue(updatedSavedSearchPageItems.Any(item => string.Equals(item.Title, "Add Saved Search", StringComparison.Ordinal)));

        // Assert the search is not in the top level commands in the CommandsProvider
        var updatedTopLevelCommands = commandsProvider.TopLevelCommands();
        Assert.IsFalse(updatedTopLevelCommands.Any(c => string.Equals(c.Title, testSearchName, StringComparison.Ordinal)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    public string? CreateJsonPayload(string enteredSearch, string name, bool isTopLevel)
    {
        return JsonNode.Parse($@"
        {{
            ""EnteredSearch"": ""{enteredSearch}"",
            ""Name"": ""{name}"",
            ""IsTopLevel"": ""{isTopLevel.ToString().ToLowerInvariant()}""
        }}")?.ToString();
    }

    public IDeveloperIdProvider CreateMockDeveloperIdProvider()
    {
        var mockDeveloperIdProvider = new Mock<IDeveloperIdProvider>();
        mockDeveloperIdProvider
            .Setup(provider => provider.IsSignedIn())
            .Returns(true);
        return mockDeveloperIdProvider.Object;
    }

    public IListItem CreateMockAddSearchListItem()
    {
        var mockAddSearchListItem = new Mock<IListItem>();
        mockAddSearchListItem.Setup(item => item.Title).Returns("Add Saved Search");
        return mockAddSearchListItem.Object;
    }

    public PersistentDataManager CreatePersistentDataManager(DataStoreOptions dataStoreOptions)
    {
        var stubValidator = new Mock<IGitHubValidator>().Object;
        return new PersistentDataManager(stubValidator, dataStoreOptions);
    }

    public GitHubExtensionCommandsProvider CreateGitHubExtensionCommandsProvider(IDeveloperIdProvider mockDeveloperIdProvider, IResources mockResources, SavedSearchesPage savedSearchesPage, PersistentDataManager persistentDataManager, SavedSearchesMediator savedSearchesMediator, ISearchPageFactory searchPageFactory)
    {
        var mockAuthenticationMediator = new Mock<AuthenticationMediator>().Object;
        var mockSignOutCommand = new Mock<SignOutCommand>(mockResources, mockDeveloperIdProvider, mockAuthenticationMediator).Object;
        var mockSignInCommand = new Mock<SignInCommand>(mockResources, mockDeveloperIdProvider, mockAuthenticationMediator).Object;
        var mockSignOutForm = new Mock<SignOutForm>(mockResources, mockAuthenticationMediator, mockSignOutCommand, mockDeveloperIdProvider).Object;
        var mockSignInForm = new Mock<SignInForm>(mockAuthenticationMediator, mockResources, mockDeveloperIdProvider, mockSignInCommand).Object;
        var signOutPage = new SignOutPage(mockResources, mockSignOutForm, mockSignOutCommand, mockAuthenticationMediator);
        var signInPage = new SignInPage(mockSignInForm, mockResources, mockSignInCommand, mockAuthenticationMediator);
        return new GitHubExtensionCommandsProvider(savedSearchesPage, signOutPage, signInPage, mockDeveloperIdProvider, persistentDataManager, mockResources, searchPageFactory, savedSearchesMediator, mockAuthenticationMediator);
    }
}
