// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Nodes;
using GitHubExtension.Controls;
using GitHubExtension.Controls.Commands;
using GitHubExtension.Controls.Forms;
using GitHubExtension.Controls.Pages;
using GitHubExtension.DataModel;
using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using GitHubExtension.PersistentData;
using GitHubExtension.Test.PersistentData;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Moq;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class TopLevelSearchesTest
{
    [TestMethod]
    public async Task SaveSearchForm_ShouldRememberIfSearchIsTopLevelWhenEditing()
    {
        // Initialize
        SearchCandidate? capturedSearchCandidate = null;
        ISearch? capturedSearch = null;

        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository
            .Setup(repo => repo.UpdateSearchTopLevelStatus(It.IsAny<ISearch>(), It.IsAny<bool>()))
            .Callback<ISearch, bool>((s, isTopLevel) =>
            {
                // ISearchRepository always returns ISearch, but the PersistentDataManager also returns SearchCandidate,
                // which is a subclass of ISearch. This test requires both.
                if (s is SearchCandidate searchCandidate)
                {
                    capturedSearchCandidate = searchCandidate;
                    capturedSearchCandidate.SearchString = s.SearchString;
                    capturedSearchCandidate.Name = s.Name;
                    capturedSearchCandidate.IsTopLevel = searchCandidate.IsTopLevel;
                }

                if (s is ISearch search)
                {
                    capturedSearch = search;
                }
            });

        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources, savedSearchesMediator);

        // Create a top-level command and save it via SaveSearchForm
        var testSearchString = "is:issue author:testuser";
        var testSearchName = "Test Search";
        var jsonPayload = CreateJsonPayload(testSearchString, testSearchName, true);

        saveSearchForm.SubmitForm(jsonPayload, string.Empty);

        Thread.Sleep(1000);

        // Assert that the search is saved and is top level in the ISearchRepository
        mockSearchRepository.Verify(
            repo =>
            repo.UpdateSearchTopLevelStatus(It.IsAny<ISearch>(), It.IsAny<bool>()),
            Times.Once);

        // Assert that the search is saved properly in the ISearchRepository
        Assert.IsNotNull(capturedSearchCandidate);
        Assert.AreEqual(testSearchName, capturedSearchCandidate.Name);
        Assert.AreEqual(testSearchString, capturedSearchCandidate.SearchString);
        Assert.IsTrue(capturedSearchCandidate.IsTopLevel);
        Assert.IsNotNull(capturedSearch);

        // Simulate creating a SaveSearchForm for the EditSearchPage. Verify that the IsTopLevel box is checked by checking the GetIsTopLevel on SaveSearchForm.
        var editSearchForm = new SaveSearchForm(capturedSearch, mockSearchRepository.Object, mockResources, savedSearchesMediator);
        mockSearchRepository
            .Setup(repo => repo.IsTopLevel(It.IsAny<ISearch>()))
            .Returns((ISearch search) => Task.FromResult(search == capturedSearch && capturedSearchCandidate.IsTopLevel));
        Assert.IsTrue(await editSearchForm.GetIsTopLevel());
    }

    // This test verifies that the SaveSearchForm properly updates the top-level status
    // of a search in the PersistentDataManager when the "IsTopLevel" checkbox is unchecked.
    [TestMethod]
    public async Task SaveSearchForm_ShouldRemoveFromTopLevel_WhenIsTopLevelUnchecked()
    {
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        var stubValidator = new Mock<IGitHubValidator>().Object;
        using var persistentDataManager = new PersistentDataManager(stubValidator, dataStoreOptions);

        // part 1: Create a top-level search and verify that it's listed as top-level
        var dummySearch = new SearchCandidate("dummy search", "Dummy Search", true);

        await persistentDataManager.UpdateSearchTopLevelStatus(dummySearch, true);

        var stubResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(dummySearch, persistentDataManager, stubResources, savedSearchesMediator);

        var initialTopLevelSearches = await persistentDataManager.GetTopLevelSearches();
        Assert.IsTrue(initialTopLevelSearches.Any(s => s.Name == dummySearch.Name && s.SearchString == dummySearch.SearchString));
        Assert.IsTrue(saveSearchForm.GetIsTopLevel().Result);

        // part 2: Uncheck the "IsTopLevel" checkbox and verify that the search is no longer top-level
        var jsonPayload = CreateJsonPayload(dummySearch.SearchString, dummySearch.Name, false);
        saveSearchForm.SubmitForm(jsonPayload, string.Empty);

        Thread.Sleep(1000);

        var updatedTopLevelSearches = await persistentDataManager.GetTopLevelSearches();
        Assert.IsFalse(updatedTopLevelSearches.Any(s => s.Name == dummySearch.Name && s.SearchString == dummySearch.SearchString));

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

        Thread.Sleep(5000);

        // Assert that search is saved and is top level in the persistentDataManager
        var savedSearches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(
            savedSearches.Any(s =>
            s.Name == testSearchName &&
            s.SearchString == testSearchString),
            "The new search should appear in saved searches");

        var persitentDataManagerTopLevelCommands = await persistentDataManager.GetTopLevelSearches();
        Assert.IsTrue(
            persitentDataManagerTopLevelCommands.Any(s =>
            s.Name == testSearchName &&
            s.SearchString == testSearchString),
            "The new search should appear in top level commands");

        // Assert that search is displayed in the saved searches page
        var savedSearchesItems = savedSearchesPage.GetItems();
        Assert.IsTrue(savedSearchesItems.Length == 2, "Should have our saved search and the add item");
        Assert.IsTrue(savedSearchesItems.Any(item => item.Title == testSearchName));

        // Assert that search is in the CommandsProvider's top level commands
        var topLevelCommands = commandsProvider.TopLevelCommands();
        Assert.IsTrue(topLevelCommands.Any(c => c.Title == testSearchName));

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

        await Task.Delay(5000);

        // Assert saved search is in PersistentDataManager's saved searches, but not top level commands
        var savedSearches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(savedSearches.Count() == 1, "Should have only our saved search");
        Assert.IsTrue(
            savedSearches.Any(s =>
                s.Name == testSearchName &&
                s.SearchString == testSearchString),
            "The new search should appear in saved searches");
        var savedSearch = savedSearches.First();
        Assert.IsFalse(
            await persistentDataManager.IsTopLevel(savedSearch));

        // Assert saved search is in SavedSearchesPage
        var savedItems = savedSearchesPage.GetItems();
        Assert.IsTrue(savedItems.Length == 2, "Should have our saved search and the add item");
        Assert.IsTrue(savedItems.Any(item => item.Title == testSearchName));
        Assert.IsTrue(savedItems.Any(item => item.Title == "Add Saved Search"));

        // Edit saved search to be on the top level
        var editSearchForm = new SaveSearchForm(savedSearch, persistentDataManager, mockResources, savedSearchesMediator);
        var editSearchString = "is:issue author:testuser";
        var editSearchName = "My Regular Search - Now top level";
        var editJsonPayload = CreateJsonPayload(editSearchString, editSearchName, true);

        editSearchForm.SubmitForm(editJsonPayload, string.Empty);

        await Task.Delay(5000);

        // Assert the saved search is updated as top level in the PersistentDataManager
        var updatedSavedSearches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(updatedSavedSearches.Count() == 1, "Should have only our saved search");
        Assert.IsTrue(
            updatedSavedSearches.Any(s =>
                s.Name == editSearchName &&
                s.SearchString == editSearchString),
            "The edited search should be saved");

        var topLevelCommands = await persistentDataManager.GetTopLevelSearches();
        Assert.IsTrue(
            topLevelCommands.Any(s =>
                s.Name == editSearchName &&
                s.SearchString == editSearchString),
            "The search should now appear in top level commands after editing");

        // Assert the saved search now appears in the top level commands
        var topLevelCommandsInCommandsProvider = commandsProvider.TopLevelCommands();
        Assert.IsTrue(topLevelCommandsInCommandsProvider.Any(c => c.Title == editSearchName));

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

        await Task.Delay(5000);

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
                s.Name == testSearchName &&
                s.SearchString == testSearchString),
            "Search should be in top level searches initially");

        // Only one saved search on the SavedSearchesPage and the add search item
        var savedSearchesItems = savedSearchesPage.GetItems();
        Assert.IsTrue(savedSearchesItems.Length == 2, "Should have our saved search and the add item");
        Assert.IsTrue(savedSearchesItems.Any(item => item.Title == testSearchName));
        Assert.IsTrue(savedSearchesItems.Any(item => item.Title == "Add Saved Search"));

        // The saved search is on the top level
        var topLevelCommands = commandsProvider.TopLevelCommands();
        Assert.IsTrue(topLevelCommands.Any(c => c.Title == testSearchName));

        // Remove the top level search
        var removeCommand = new RemoveSavedSearchCommand(topLevelSearch, persistentDataManager, mockResources, savedSearchesMediator);
        removeCommand.Invoke();

        await Task.Delay(5000);

        // Assert the search is removed from the PersistentDataManager's saved searches and top level searches
        var updatedSavedSearches = await persistentDataManager.GetSavedSearches();
        Assert.IsFalse(
            updatedSavedSearches.Any(s =>
                s.Name == testSearchName &&
                s.SearchString == testSearchString),
            "Search should be removed from saved searches");

        var updatedTopLevelSearches = await persistentDataManager.GetTopLevelSearches();
        Assert.IsFalse(
            updatedTopLevelSearches.Any(s =>
                s.Name == testSearchName &&
                s.SearchString == testSearchString),
            "Search should be removed from top level searches");

        // Assert the search is removed from the SavedSearchesPage
        var updatedSavedSearchesItems = savedSearchesPage.GetItems();
        Assert.IsTrue(updatedSavedSearchesItems.Length == 1, "Should only have the add item");
        Assert.IsFalse(updatedSavedSearchesItems.Any(item => item.Title == testSearchName));

        // Assert the search is removed from the top level commands in the CommandsProvider
        var updatedTopLevelCommands = commandsProvider.TopLevelCommands();
        Assert.IsFalse(updatedTopLevelCommands.Any(c => c.Title == testSearchName));

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

        Thread.Sleep(5000);

        // Ensure the test conditions are set up correctly:
        var initialSavedSearches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(initialSavedSearches.Count() == 1, "Should have only our saved search");
        Assert.IsTrue(
            initialSavedSearches.Any(s =>
                s.Name == testSearchName &&
                s.SearchString == testSearchString),
            "Search should be in saved searches initially");

        var initialTopLevelSearches = await persistentDataManager.GetTopLevelSearches();
        Assert.IsTrue(initialTopLevelSearches.Count() == 1, "Should have only our saved search");
        Assert.IsTrue(
            initialTopLevelSearches.Any(s =>
                s.Name == testSearchName &&
                s.SearchString == testSearchString),
            "Search should be in top level searches initially");

        var savedSearchesItems = savedSearchesPage.GetItems();
        Assert.IsTrue(savedSearchesItems.Length == 2, "Should have our saved search and the add item");
        Assert.IsTrue(savedSearchesItems.Any(item => item.Title == testSearchName));
        Assert.IsTrue(savedSearchesItems.Any(item => item.Title == "Add Saved Search"));

        var topLevelCommands = commandsProvider.TopLevelCommands();
        Assert.IsTrue(topLevelCommands.Any(c => c.Title == testSearchName));

        // Edit the saved search to be non-top-level
        var editSearchForm = new SaveSearchForm(testSearch, persistentDataManager, mockResources, savedSearchesMediator);

        var editJsonPayload = CreateJsonPayload(testSearchString, testSearchName, false);

        editSearchForm.SubmitForm(editJsonPayload, string.Empty);

        await Task.Delay(5000);

        // Assert the saved search is updated as non-top level in the PersistentDataManager
        var updatedSavedSearches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(
            updatedSavedSearches.Any(s =>
                s.Name == testSearch.Name &&
                s.SearchString == testSearch.SearchString),
            "The search should still appear in saved searches after editing");

        var updatedTopLevelSearches = await persistentDataManager.GetTopLevelSearches();
        Assert.IsFalse(
            updatedTopLevelSearches.Any(s =>
                s.Name == testSearchName &&
                s.SearchString == testSearch.SearchString),
            "The search should not appear in top level commands after editing");

        // Assert the search is still in the SavedSearchesPage
        var updatedSavedSearchPageItems = savedSearchesPage.GetItems();
        Assert.IsTrue(updatedSavedSearchPageItems.Length == 2, "Should have our saved search and the add item");
        Assert.IsTrue(updatedSavedSearchPageItems.Any(item => item.Title == testSearchName));
        Assert.IsTrue(updatedSavedSearchPageItems.Any(item => item.Title == "Add Saved Search"));

        // Assert the search is not in the top level commands in the CommandsProvider
        var updatedTopLevelCommands = commandsProvider.TopLevelCommands();
        Assert.IsFalse(updatedTopLevelCommands.Any(c => c.Title == testSearchName));

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
        var mockSignOutForm = new Mock<SignOutForm>(mockDeveloperIdProvider, mockResources, mockAuthenticationMediator).Object;
        var mockSignInForm = new Mock<SignInForm>(mockDeveloperIdProvider, mockResources, mockAuthenticationMediator).Object;
        var signOutPage = new SignOutPage(mockSignOutForm, new StatusMessage(), mockResources.GetResource("Message_Sign_Out_Success"), mockResources.GetResource("Message_Sign_Out_Fail"));
        var signInPage = new SignInPage(mockSignInForm, new StatusMessage(), mockResources.GetResource("Message_Sign_In_Success"), mockResources.GetResource("Message_Sign_In_Fail"));
        return new GitHubExtensionCommandsProvider(savedSearchesPage, signOutPage, signInPage, mockDeveloperIdProvider, persistentDataManager, mockResources, searchPageFactory, savedSearchesMediator, mockAuthenticationMediator);
    }
}
