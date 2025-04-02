// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Nodes;
using GitHubExtension.Client;
using GitHubExtension.Controls;
using GitHubExtension.Controls.Commands;
using GitHubExtension.Controls.Forms;
using GitHubExtension.Controls.ListItems;
using GitHubExtension.Controls.Pages;
using GitHubExtension.DataManager;
using GitHubExtension.DataManager.Cache;
using GitHubExtension.DataManager.Data;
using GitHubExtension.DataModel;
using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using GitHubExtension.PersistentData;
using GitHubExtension.Test.PersistentData;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Microsoft.Windows.ApplicationModel.Resources;
using Moq;
using Windows.Media.Audio;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class TopLevelSearchesTest
{
    public string? CreateJsonPayload(string enteredSearch, string name, bool isTopLevel)
    {
        return JsonNode.Parse($@"
        {{
            ""EnteredSearch"": ""{enteredSearch}"",
            ""Name"": ""{name}"",
            ""IsTopLevel"": ""{isTopLevel.ToString().ToLowerInvariant()}""
        }}")?.ToString();
    }

    // This test verifies that the SaveSearchForm properly updates the top-level status
    // when the "IsTopLevel" checkbox is checked.
    [TestMethod]
    public async Task SaveSearchForm_ShouldRetainIsTopLevel_WhenSearchIsSaved()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository
            .Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        SearchCandidate? capturedSearchCandidate = null;
        ISearch? capturedSearch = null;
        mockSearchRepository
            .Setup(repo => repo.UpdateSearchTopLevelStatus(It.IsAny<ISearch>(), It.IsAny<bool>()))
            .Callback<ISearch, bool>((s, isTopLevel) =>
            {
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

        var stubResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, stubResources, savedSearchesMediator);

        var jsonPayload = CreateJsonPayload("test search", "test name", true);

        saveSearchForm.SubmitForm(jsonPayload, string.Empty);

        Thread.Sleep(1000);

        mockSearchRepository.Verify(
            repo =>
            repo.UpdateSearchTopLevelStatus(It.IsAny<ISearch>(), It.IsAny<bool>()),
            Times.Once);

        Assert.IsNotNull(capturedSearchCandidate);
        Assert.AreEqual("test name", capturedSearchCandidate.Name);
        Assert.AreEqual("test search", capturedSearchCandidate.SearchString);
        Assert.IsTrue(capturedSearchCandidate.IsTopLevel);
        Assert.IsNotNull(capturedSearch);

        // Simulate creating a SaveSearchForm for the EditSearchPage. Verify that the IsTopLevel box is checked by checking the GetIsTopLevel on SaveSearchForm.
        var saveSearchForm2 = new SaveSearchForm(capturedSearch, mockSearchRepository.Object, stubResources, savedSearchesMediator);
        mockSearchRepository
            .Setup(repo => repo.IsTopLevel(It.IsAny<ISearch>()))
            .Returns((ISearch search) => Task.FromResult(search == capturedSearch && capturedSearchCandidate.IsTopLevel));
        Assert.IsTrue(await saveSearchForm2.GetIsTopLevel());
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
    public async Task Integration_ShouldRemoveTopLevelCommand_FromTopLevel()
    {
        var mockSearchPageFactory = new Mock<ISearchPageFactory>();
        mockSearchPageFactory
            .Setup(factory => factory.CreateItemForSearch(It.IsAny<ISearch>()))
            .Returns((ISearch search) =>
            {
                var mockListItem = new Mock<IListItem>();
                mockListItem.Setup(item => item.Title).Returns(search.Name);
                return mockListItem.Object;
            });

        var mockAddSearchListItem = new Mock<IListItem>();
        mockAddSearchListItem.Setup(item => item.Title).Returns("Add Saved Search");

        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        var stubValidator = new Mock<IGitHubValidator>().Object;

        using var persistentDataManager = new PersistentDataManager(stubValidator, dataStoreOptions);

        var topLevelSearch = new SearchCandidate("is:issue assignee:me", "Important Issues", true);
        await persistentDataManager.UpdateSearchTopLevelStatus(topLevelSearch, true);

        var initialSavedSearches = await persistentDataManager.GetSavedSearches();
        var initialTopLevelSearches = await persistentDataManager.GetTopLevelSearches();

        Assert.IsTrue(
            initialSavedSearches.Any(s =>
                s.Name == "Important Issues" &&
                s.SearchString == "is:issue assignee:me"),
            "Search should be in saved searches initially");

        Assert.IsTrue(
            initialTopLevelSearches.Any(s =>
                s.Name == "Important Issues" &&
                s.SearchString == "is:issue assignee:me"),
            "Search should be in top level searches initially");

        var stubResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var removeCommand = new RemoveSavedSearchCommand(topLevelSearch, persistentDataManager, stubResources, savedSearchesMediator);
        removeCommand.Invoke();

        await Task.Delay(1000);

        var updatedTopLevelSearches = await persistentDataManager.GetTopLevelSearches();
        Assert.IsFalse(
            updatedTopLevelSearches.Any(s =>
                s.Name == "Important Issues" &&
                s.SearchString == "is:issue assignee:me"),
            "Search should be removed from top level searches");

        var updatedSavedSearches = await persistentDataManager.GetSavedSearches();
        Assert.IsFalse(
            updatedSavedSearches.Any(s =>
                s.Name == "Important Issues" &&
                s.SearchString == "is:issue assignee:me"),
            "Search should also be removed from saved searches");

        var savedSearchesPage = new SavedSearchesPage(
            mockSearchPageFactory.Object,
            persistentDataManager,
            stubResources,
            mockAddSearchListItem.Object,
            savedSearchesMediator);

        var savedSearchesItems = savedSearchesPage.GetItems();
        var containsRemovedSearch = false;
        foreach (var item in savedSearchesItems)
        {
            if (item.Title == "Important Issues")
            {
                containsRemovedSearch = true;
                break;
            }
        }

        Assert.IsFalse(containsRemovedSearch, "Removed search should not appear in saved searches page items");

        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task Integration_ShouldEditNonTopLevelSearch_ToBeTopLevel()
    {
        var mockSearchPageFactory = new Mock<ISearchPageFactory>();
        mockSearchPageFactory
            .Setup(factory => factory.CreateItemForSearch(It.IsAny<ISearch>()))
            .Returns((ISearch search) =>
            {
                var mockListItem = new Mock<IListItem>();
                mockListItem.Setup(item => item.Title).Returns(search.Name);
                return mockListItem.Object;
            });

        var mockAddSearchListItem = new Mock<IListItem>();
        mockAddSearchListItem.Setup(item => item.Title).Returns("Add Saved Search");

        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        var stubValidator = new Mock<IGitHubValidator>().Object;

        using var persistentDataManager = new PersistentDataManager(stubValidator, dataStoreOptions);

        var regularSearch = new SearchCandidate("is:issue label:bug", "Bug Reports", false);
        await persistentDataManager.UpdateSearchTopLevelStatus(regularSearch, false);

        // Verify initial state (search in saved searches but not in top level)
        var initialSavedSearches = await persistentDataManager.GetSavedSearches();
        var initialTopLevelSearches = await persistentDataManager.GetTopLevelSearches();

        Assert.IsTrue(
            initialSavedSearches.Any(s =>
                s.Name == "Bug Reports" &&
                s.SearchString == "is:issue label:bug"),
            "Search should be in saved searches initially");

        Assert.IsFalse(
            initialTopLevelSearches.Any(s =>
                s.Name == "Bug Reports" &&
                s.SearchString == "is:issue label:bug"),
            "Search should not be in top level searches initially");

        var savedSearchesMediator = new SavedSearchesMediator();
        var stubResources = new Mock<IResources>().Object;
        var savedSearchesPage = new SavedSearchesPage(
            mockSearchPageFactory.Object,
            persistentDataManager,
            stubResources,
            mockAddSearchListItem.Object,
            savedSearchesMediator);

        var savedSearch = initialSavedSearches.First(s =>
            s.Name == "Bug Reports" &&
            s.SearchString == "is:issue label:bug");

        var editSearchForm = new SaveSearchForm(savedSearch, persistentDataManager, stubResources, savedSearchesMediator);

        var editJsonPayload = JsonNode.Parse(@"
        {
            ""EnteredSearch"": ""is:issue label:bug"",
            ""Name"": ""Bug Reports"",
            ""IsTopLevel"": ""true""
        }")?.ToString();

        editSearchForm.SubmitForm(editJsonPayload, string.Empty);

        await Task.Delay(1000);

        var updatedSavedSearches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(
            updatedSavedSearches.Any(s =>
                s.Name == "Bug Reports" &&
                s.SearchString == "is:issue label:bug"),
            "The search should still appear in saved searches after editing");

        var updatedTopLevelSearches = await persistentDataManager.GetTopLevelSearches();
        Assert.IsTrue(
            updatedTopLevelSearches.Any(s =>
                s.Name == "Bug Reports" &&
                s.SearchString == "is:issue label:bug"),
            "The search should now appear in top level commands after editing");

        var savedSearchesItems = savedSearchesPage.GetItems();
        var containsUpdatedSearch = false;
        foreach (var item in savedSearchesItems)
        {
            if (item.Title == "Bug Reports")
            {
                containsUpdatedSearch = true;
                break;
            }
        }

        Assert.IsTrue(containsUpdatedSearch, "Updated search should appear in saved searches page items");

        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }
}
