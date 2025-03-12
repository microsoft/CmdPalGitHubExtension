// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Nodes;
using GitHubExtension.Controls;
using GitHubExtension.Controls.Commands;
using GitHubExtension.Controls.Forms;
using GitHubExtension.Controls.Pages;
using GitHubExtension.DataModel;
using GitHubExtension.Helpers;
using GitHubExtension.PersistentData;
using Microsoft.CommandPalette.Extensions;
using Moq;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class TopLevelSearchesTest
{
    [TestMethod]
    public async Task SaveSearchForm_ShouldRetainIsTopLevel_WhenSearchIsSaved()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository
            .Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        mockSearchRepository
            .Setup(repo => repo.RemoveSavedSearch(It.IsAny<ISearch>()))
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
                    capturedSearchCandidate.IsTopLevel = isTopLevel;
                }

                if (s is ISearch search)
                {
                    capturedSearch = search;
                }
            });

        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object);

        var jsonPayload = JsonNode.Parse(@"
            {
                ""EnteredSearch"": ""test search"",
                ""Name"": ""test name"",
                ""IsTopLevel"": ""true""
            }")?.ToString();

        saveSearchForm.SubmitForm(jsonPayload, string.Empty);

        Thread.Sleep(1000);

        mockSearchRepository.Verify(
            repo =>
            repo.UpdateSearchTopLevelStatus(It.IsAny<ISearch>(), It.IsAny<bool>()),
            Times.Once);

        Task.Delay(2000).Wait();

        Assert.IsNotNull(capturedSearchCandidate);
        Assert.AreEqual("test name", capturedSearchCandidate.Name);
        Assert.AreEqual("test search", capturedSearchCandidate.SearchString);
        Assert.IsTrue(capturedSearchCandidate.IsTopLevel);

        Assert.IsNotNull(capturedSearch);

        var saveSearchForm2 = new SaveSearchForm(capturedSearch, mockSearchRepository.Object);
        mockSearchRepository
            .Setup(repo => repo.IsTopLevel(capturedSearch))
            .Returns(Task.FromResult(true));
        Assert.IsTrue(await saveSearchForm2.GetIsTopLevel());
    }

    // This test verifies that the SaveSearchForm properly updates the top-level status
    // of a search in the PersistentDataManager when the "IsTopLevel" checkbox is unchecked.
    [TestMethod]
    public async Task SaveSearchForm_ShouldRemoveFromTopLevel_WhenIsTopLevelUnchecked()
    {
        var mockGitHubValidator = new Mock<IGitHubValidator>();
        mockGitHubValidator
            .Setup(validator => validator.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        var mockDataStoreOptions = new Mock<DataStoreOptions>();
        mockDataStoreOptions.SetupAllProperties();
        mockDataStoreOptions.Object.DataStoreFolderPath = Path.GetTempPath();
        mockDataStoreOptions.Object.DataStoreSchema = new PersistentDataSchema();

        using var persistentDataManager = new PersistentDataManager(mockGitHubValidator.Object, mockDataStoreOptions.Object);

        var dummySearch = new SearchCandidate("dummy search 2", "Dummy Search", true);
        await persistentDataManager.UpdateSearchTopLevelStatus(dummySearch, true);

        var saveSearchForm = new SaveSearchForm(dummySearch, persistentDataManager);

        var initialTopLevelSearches = await persistentDataManager.GetTopLevelSearches();
        Assert.IsTrue(initialTopLevelSearches.Any(s => s.Name == "Dummy Search" && s.SearchString == "dummy search 2"));

        var jsonPayload = JsonNode.Parse(@"
        {
            ""EnteredSearch"": ""dummy search"",
            ""Name"": ""Dummy Search"",
            ""IsTopLevel"": ""false""
        }")?.ToString();

        saveSearchForm.SubmitForm(jsonPayload, string.Empty);

        Thread.Sleep(1000);

        var updatedTopLevelSearches = await persistentDataManager.GetTopLevelSearches();
        Assert.IsFalse(updatedTopLevelSearches.Any(s => s.Name == "Dummy Search" && s.SearchString == "dummy search"));

        var isTopLevel = await persistentDataManager.IsTopLevel(dummySearch);
        Assert.IsFalse(isTopLevel);
    }

    [TestMethod]
    public async Task Integration_ShouldAddTopLevelCommand_FromSavedSearchesPage()
    {
        var mockGitHubValidator = new Mock<IGitHubValidator>();
        mockGitHubValidator
            .Setup(validator => validator.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository
            .Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        mockSearchRepository
            .Setup(repo => repo.RemoveSavedSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        mockSearchRepository
            .Setup(repo => repo.UpdateSearchTopLevelStatus(It.IsAny<ISearch>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

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

        var mockDataStoreOptions = new Mock<DataStoreOptions>();
        mockDataStoreOptions.SetupAllProperties();
        mockDataStoreOptions.Object.DataStoreFolderPath = Path.GetTempPath();
        mockDataStoreOptions.Object.DataStoreSchema = new PersistentDataSchema();

        using var persistentDataManager = new PersistentDataManager(mockGitHubValidator.Object, mockDataStoreOptions.Object);

        var successMessageReceived = false;
        SaveSearchForm.SearchSaved += (sender, args) =>
        {
            if (args is ISearch search && search.Name == "New Top Level Search")
            {
                successMessageReceived = true;
            }
        };

        var savedSearchesPage = new SavedSearchesPage(
            mockSearchPageFactory.Object,
            persistentDataManager,
            mockAddSearchListItem.Object);

        var saveSearchForm = new SaveSearchForm(persistentDataManager);

        var jsonPayload = JsonNode.Parse(@"
        {
            ""EnteredSearch"": ""is:issue author:testuser"",
            ""Name"": ""New Top Level Search"",
            ""IsTopLevel"": ""true""
        }")?.ToString();

        saveSearchForm.SubmitForm(jsonPayload, string.Empty);

        await Task.Delay(1000);

        Assert.IsTrue(successMessageReceived, "Success message should be received when adding a top level command");

        var savedSearches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(
            savedSearches.Any(s =>
            s.Name == "New Top Level Search" &&
            s.SearchString == "is:issue author:testuser"),
            "The new search should appear in saved searches");

        var topLevelCommands = await persistentDataManager.GetTopLevelSearches();
        Assert.IsTrue(
            topLevelCommands.Any(s =>
            s.Name == "New Top Level Search" &&
            s.SearchString == "is:issue author:testuser"),
            "The new search should appear in top level commands");

        var search = savedSearches.FirstOrDefault(s => s.Name == "New Top Level Search");
        Assert.IsNotNull(search, "Search should exist");

        var isTopLevel = await persistentDataManager.IsTopLevel(search);
        Assert.IsTrue(isTopLevel, "Search should be marked as top level");

        SaveSearchForm.SearchSaved -= (sender, args) => { };
    }

    [TestMethod]
    public async Task Integration_ShouldAddAndEditSearch_ToBeTopLevel()
    {
        var mockGitHubValidator = new Mock<IGitHubValidator>();
        mockGitHubValidator
            .Setup(validator => validator.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

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

        var mockDataStoreOptions = new Mock<DataStoreOptions>();
        mockDataStoreOptions.SetupAllProperties();
        mockDataStoreOptions.Object.DataStoreFolderPath = Path.GetTempPath();
        mockDataStoreOptions.Object.DataStoreSchema = new PersistentDataSchema();

        using var persistentDataManager = new PersistentDataManager(mockGitHubValidator.Object, mockDataStoreOptions.Object);

        var addSuccessMessageReceived = false;
        var editSuccessMessageReceived = false;

        SaveSearchForm.SearchSaved += (sender, args) =>
        {
            if (args is ISearch search)
            {
                if (search.Name == "My Regular Search" && !addSuccessMessageReceived)
                {
                    addSuccessMessageReceived = true;
                }
                else if (search.Name == "My Regular Search" && addSuccessMessageReceived)
                {
                    editSuccessMessageReceived = true;
                }
            }
        };

        var savedSearchesPage = new SavedSearchesPage(
            mockSearchPageFactory.Object,
            persistentDataManager,
            mockAddSearchListItem.Object);

        var initialSaveSearchForm = new SaveSearchForm(persistentDataManager);

        var initialJsonPayload = JsonNode.Parse(@"
        {
            ""EnteredSearch"": ""is:issue author:testuser"",
            ""Name"": ""My Regular Search"",
            ""IsTopLevel"": ""false""
        }")?.ToString();

        initialSaveSearchForm.SubmitForm(initialJsonPayload, string.Empty);

        await Task.Delay(1000);

        Assert.IsTrue(addSuccessMessageReceived, "Success message should be received when adding a search");

        var savedItems = savedSearchesPage.GetItems();
        Assert.IsTrue(savedItems.Length > 1, "Should have at least our saved search and the add item");

        var savedSearches = await persistentDataManager.GetSavedSearches();
        var savedSearch = savedSearches.FirstOrDefault(s => s.Name == "My Regular Search");
        Assert.IsNotNull(savedSearch, "Saved search should exist");

        var editSearchForm = new SaveSearchForm(savedSearch, persistentDataManager);

        var editJsonPayload = JsonNode.Parse(@"
        {
            ""EnteredSearch"": ""is:issue author:testuser"",
            ""Name"": ""My Regular Search"",
            ""IsTopLevel"": ""true""
        }")?.ToString();

        editSearchForm.SubmitForm(editJsonPayload, string.Empty);

        await Task.Delay(1000);

        Assert.IsTrue(editSuccessMessageReceived, "Success message should be received when editing a search to be top-level");

        var updatedSavedSearches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(
            updatedSavedSearches.Any(s =>
                s.Name == "My Regular Search" &&
                s.SearchString == "is:issue author:testuser"),
            "The search should still appear in saved searches after editing");

        var topLevelCommands = await persistentDataManager.GetTopLevelSearches();
        Assert.IsTrue(
            topLevelCommands.Any(s =>
                s.Name == "My Regular Search" &&
                s.SearchString == "is:issue author:testuser"),
            "The search should now appear in top level commands after editing");

        SaveSearchForm.SearchSaved -= (sender, args) => { };
    }

    [TestMethod]
    public async Task Integration_ShouldRemoveTopLevelCommand_FromSavedSearchesPage()
    {
        var mockGitHubValidator = new Mock<IGitHubValidator>();
        mockGitHubValidator
            .Setup(validator => validator.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

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

        var mockDataStoreOptions = new Mock<DataStoreOptions>();
        mockDataStoreOptions.SetupAllProperties();
        mockDataStoreOptions.Object.DataStoreFolderPath = Path.GetTempPath();
        mockDataStoreOptions.Object.DataStoreSchema = new PersistentDataSchema();

        using var persistentDataManager = new PersistentDataManager(mockGitHubValidator.Object, mockDataStoreOptions.Object);

        var searchRemoved = false;
        RemoveSavedSearchCommand.SearchRemoved += (sender, args) =>
        {
            if (args is bool success && success)
            {
                searchRemoved = true;
            }
        };

        var topLevelSearch = new SearchCandidate("is:issue author:testuser", "Top Level Search", true);
        await persistentDataManager.UpdateSearchTopLevelStatus(topLevelSearch, true);

        var initialSavedSearches = await persistentDataManager.GetSavedSearches();
        var initialTopLevelSearches = await persistentDataManager.GetTopLevelSearches();

        Assert.IsTrue(
            initialSavedSearches.Any(s =>
                s.Name == "Top Level Search" &&
                s.SearchString == "is:issue author:testuser"),
            "Search should be in saved searches initially");

        Assert.IsTrue(
            initialTopLevelSearches.Any(s =>
                s.Name == "Top Level Search" &&
                s.SearchString == "is:issue author:testuser"),
            "Search should be in top level searches initially");

        var savedSearchesPage = new SavedSearchesPage(
            mockSearchPageFactory.Object,
            persistentDataManager,
            mockAddSearchListItem.Object);

        var removeCommand = new RemoveSavedSearchCommand(topLevelSearch, persistentDataManager);
        removeCommand.Invoke();

        await Task.Delay(1000);

        Assert.IsTrue(searchRemoved, "Search removal should signal success");

        var updatedSavedSearches = await persistentDataManager.GetSavedSearches();
        Assert.IsFalse(
            updatedSavedSearches.Any(s =>
                s.Name == "Top Level Search" &&
                s.SearchString == "is:issue author:testuser"),
            "Search should be removed from saved searches");

        var updatedTopLevelSearches = await persistentDataManager.GetTopLevelSearches();
        Assert.IsFalse(
            updatedTopLevelSearches.Any(s =>
                s.Name == "Top Level Search" &&
                s.SearchString == "is:issue author:testuser"),
            "Search should be removed from top level searches");

        RemoveSavedSearchCommand.SearchRemoved -= (sender, args) => { };
    }

    [TestMethod]
    public async Task Integration_ShouldRemoveTopLevelCommand_FromTopLevel()
    {
        var mockGitHubValidator = new Mock<IGitHubValidator>();
        mockGitHubValidator
            .Setup(validator => validator.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

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

        var mockDataStoreOptions = new Mock<DataStoreOptions>();
        mockDataStoreOptions.SetupAllProperties();
        mockDataStoreOptions.Object.DataStoreFolderPath = Path.GetTempPath();
        mockDataStoreOptions.Object.DataStoreSchema = new PersistentDataSchema();

        using var persistentDataManager = new PersistentDataManager(mockGitHubValidator.Object, mockDataStoreOptions.Object);

        var searchRemoved = false;
        RemoveSavedSearchCommand.SearchRemoved += (sender, args) =>
        {
            if (args is bool success && success)
            {
                searchRemoved = true;
            }
        };

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

        var removeCommand = new RemoveSavedSearchCommand(topLevelSearch, persistentDataManager);
        removeCommand.Invoke();

        await Task.Delay(1000);

        Assert.IsTrue(searchRemoved, "Search removal should signal success");

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
            mockAddSearchListItem.Object);

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

        RemoveSavedSearchCommand.SearchRemoved -= (sender, args) => { };
    }

    [TestMethod]
    public async Task Integration_ShouldEditNonTopLevelSearch_ToBeTopLevel()
    {
        var mockGitHubValidator = new Mock<IGitHubValidator>();
        mockGitHubValidator
            .Setup(validator => validator.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

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

        var mockDataStoreOptions = new Mock<DataStoreOptions>();
        mockDataStoreOptions.SetupAllProperties();
        mockDataStoreOptions.Object.DataStoreFolderPath = Path.GetTempPath();
        mockDataStoreOptions.Object.DataStoreSchema = new PersistentDataSchema();

        using var persistentDataManager = new PersistentDataManager(mockGitHubValidator.Object, mockDataStoreOptions.Object);

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

        // Step 1: Navigate to Saved Searches Page
        var savedSearchesPage = new SavedSearchesPage(
            mockSearchPageFactory.Object,
            persistentDataManager,
            mockAddSearchListItem.Object);

        // Step 2: Select edit on the saved search
        var savedSearch = initialSavedSearches.First(s =>
            s.Name == "Bug Reports" &&
            s.SearchString == "is:issue label:bug");

        var editSearchForm = new SaveSearchForm(savedSearch, persistentDataManager);

        // Event tracking variables
        var loadingStarted = false;
        var loadingFinished = false;
        var formSubmitted = false;
        var formSubmitSucceeded = false;
        var searchSaved = false;

        // Track all events
        editSearchForm.LoadingStateChanged += (sender, isLoading) =>
        {
            if (isLoading)
            {
                loadingStarted = true;
            }
            else
            {
                loadingFinished = true;
            }
        };

        editSearchForm.FormSubmitted += (sender, args) =>
        {
            formSubmitted = true;
            formSubmitSucceeded = args.Status;
        };

        SaveSearchForm.SearchSaved += (sender, args) =>
        {
            if (args is ISearch search && search.Name == "Bug Reports")
            {
                searchSaved = true;
            }
        };

        // Step 3: Check the box to make the command top level
        var editJsonPayload = JsonNode.Parse(@"
        {
            ""EnteredSearch"": ""is:issue label:bug"",
            ""Name"": ""Bug Reports"",
            ""IsTopLevel"": ""true""
        }")?.ToString();

        // Submit the form to update the search as top-level
        editSearchForm.SubmitForm(editJsonPayload, string.Empty);

        // Wait for async operations to complete
        await Task.Delay(1000);

        // Verify events were triggered correctly
        Assert.IsTrue(loadingStarted, "LoadingStateChanged should be triggered with true when form submission begins");
        Assert.IsTrue(loadingFinished, "LoadingStateChanged should be triggered with false when form submission completes");
        Assert.IsTrue(formSubmitted, "FormSubmitted event should be triggered");
        Assert.IsTrue(formSubmitSucceeded, "Form submission should succeed");
        Assert.IsTrue(searchSaved, "SearchSaved event should be triggered with the updated search");

        // Verify search is still in saved searches
        var updatedSavedSearches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(
            updatedSavedSearches.Any(s =>
                s.Name == "Bug Reports" &&
                s.SearchString == "is:issue label:bug"),
            "The search should still appear in saved searches after editing");

        // Verify search is now in top-level commands
        var updatedTopLevelSearches = await persistentDataManager.GetTopLevelSearches();
        Assert.IsTrue(
            updatedTopLevelSearches.Any(s =>
                s.Name == "Bug Reports" &&
                s.SearchString == "is:issue label:bug"),
            "The search should now appear in top level commands after editing");

        // Use SavedSearchesPage to verify the event handler works properly
        savedSearchesPage.OnSearchSaved(editSearchForm, savedSearch);

        // Check that the command appears in the SavedSearchesPage items
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

        // Clean up event subscriptions
        SaveSearchForm.SearchSaved -= (sender, args) => { };
    }
}
