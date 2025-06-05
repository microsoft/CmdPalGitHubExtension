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
using Moq;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class TopLevelSearchesTest
{
    public const int DEFAULTTESTDELAYMS = 50;
    public const int DEFAULTTESTDELAYLONGMS = 500;

    private (PersistentDataManager PersistentDataManager, IResources Resources, SavedSearchesMediator Mediator, DataStoreOptions DataStoreOptions) CreateTestContext()
    {
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var resources = new Mock<IResources>().Object;
        var mediator = new SavedSearchesMediator();
        return (persistentDataManager, resources, mediator, dataStoreOptions);
    }

    [TestMethod]
    public async Task SaveSearchForm_KeepsIsTopLevelCheckedIfSearchSavedToTopLevel()
    {
        var (persistentDataManager, resources, mediator, dataStoreOptions) = CreateTestContext();
        try
        {
            var saveSearchForm = new SaveSearchForm(persistentDataManager, resources, mediator);

            var testSearchString = "is:issue author:testuser";
            var testSearchName = "Test Search";
            var jsonPayload = CreateJsonPayload(testSearchString, testSearchName, true);

            saveSearchForm.SubmitForm(jsonPayload, string.Empty);

            await Task.Delay(DEFAULTTESTDELAYLONGMS);

            var savedSearches = await persistentDataManager.GetSavedSearches();
            Assert.IsTrue(savedSearches.Count() == 1, "Should have only our saved search");
            Assert.IsTrue(savedSearches.Any(s => string.Equals(s.Name, testSearchName, StringComparison.Ordinal)), "The new search should appear in saved searches");

            var editSearchForm = new SaveSearchForm(savedSearches.First(), persistentDataManager, resources, mediator);
            Assert.IsTrue(await editSearchForm.GetIsTopLevel());
        }
        finally
        {
            persistentDataManager.Dispose();
            PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
        }
    }

    [TestMethod]
    public async Task SaveSearchForm_KeepsIsTopLevelUncheckedIfSearchIsNoLongerSavedToTopLevel()
    {
        var (persistentDataManager, mockResources, savedSearchesMediator, dataStoreOptions) = CreateTestContext();
        try
        {
            var dummySearch = new SearchCandidate("dummy search", "Dummy Search", true);

            await persistentDataManager.UpdateSearchTopLevelStatus(dummySearch, true);
            savedSearchesMediator.AddSearch(dummySearch);

            var saveSearchForm = new SaveSearchForm(dummySearch, persistentDataManager, mockResources, savedSearchesMediator);

            var initialTopLevelSearches = await persistentDataManager.GetTopLevelSearches();
            Assert.IsTrue(initialTopLevelSearches.Count() == 1, "Should have only our saved search");
            Assert.IsTrue(initialTopLevelSearches.Any(s => string.Equals(s.Name, dummySearch.Name, StringComparison.Ordinal)
                && string.Equals(s.SearchString, dummySearch.SearchString, StringComparison.Ordinal)));
            Assert.IsTrue(saveSearchForm.GetIsTopLevel().Result);

            var jsonPayload = CreateJsonPayload(dummySearch.SearchString, dummySearch.Name, false);
            saveSearchForm.SubmitForm(jsonPayload, string.Empty);

            await Task.Delay(DEFAULTTESTDELAYMS);

            var editSearchForm = new SaveSearchForm(initialTopLevelSearches.First(), persistentDataManager, mockResources, savedSearchesMediator);
            Assert.IsFalse(await editSearchForm.GetIsTopLevel());

            var updatedTopLevelSearches = await persistentDataManager.GetTopLevelSearches();
            Assert.IsFalse(updatedTopLevelSearches.Any(), "Should have no top-level searches");

            var isTopLevel = await persistentDataManager.IsTopLevel(dummySearch);
            Assert.IsFalse(isTopLevel);
            Assert.IsFalse(saveSearchForm.GetIsTopLevel().Result);
        }
        finally
        {
            persistentDataManager.Dispose();
            PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
        }
    }

    [TestMethod]
    public async Task Integration_AddNewTopLevelCommand()
    {
        var (persistentDataManager, mockResources, savedSearchesMediator, dataStoreOptions) = CreateTestContext();
        try
        {
            var mockDeveloperIdProvider = CreateMockDeveloperIdProvider();
            var mockCacheDataManager = new Mock<ICacheDataManager>().Object;
            var searchPageFactory = new SearchPageFactory(mockCacheDataManager, persistentDataManager, mockResources, savedSearchesMediator);

            var addSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

            var mockAddSearchListItem = CreateMockAddSearchListItem();
            var savedSearchesPage = new SavedSearchesPage(searchPageFactory, persistentDataManager, mockResources, mockAddSearchListItem, savedSearchesMediator);

            var commandsProvider = CreateGitHubExtensionCommandsProvider(mockDeveloperIdProvider, mockResources, savedSearchesPage, persistentDataManager, savedSearchesMediator, searchPageFactory);

            var testSearchString = "is:issue author:testuser";
            var testSearchName = "New Top Level Search";
            var jsonPayload = CreateJsonPayload(testSearchString, testSearchName, true);
            addSearchForm.SubmitForm(jsonPayload, string.Empty);

            await Task.Delay(DEFAULTTESTDELAYMS);

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

            var savedSearchesItems = savedSearchesPage.GetItems();
            Assert.IsTrue(savedSearchesItems.Length == 2, "Should have our saved search and the add item");
            Assert.IsTrue(savedSearchesItems.Any(item => string.Equals(item.Title, testSearchName, StringComparison.Ordinal)));

            var topLevelCommands = commandsProvider.TopLevelCommands();
            Assert.IsTrue(topLevelCommands.Any(c => string.Equals(c.Title, testSearchName, StringComparison.Ordinal)));
        }
        finally
        {
            persistentDataManager.Dispose();
            PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
        }
    }

    [TestMethod]
    public async Task Integration_AddAndEditSearch_ToBeTopLevel()
    {
        var (persistentDataManager, mockResources, savedSearchesMediator, dataStoreOptions) = CreateTestContext();
        try
        {
            var mockDeveloperIdProvider = CreateMockDeveloperIdProvider();
            var mockCacheDataManager = new Mock<ICacheDataManager>().Object;
            var searchPageFactory = new SearchPageFactory(mockCacheDataManager, persistentDataManager, mockResources, savedSearchesMediator);

            var addSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

            var mockAddSearchListItem = CreateMockAddSearchListItem();
            var savedSearchesPage = new SavedSearchesPage(searchPageFactory, persistentDataManager, mockResources, mockAddSearchListItem, savedSearchesMediator);

            var commandsProvider = CreateGitHubExtensionCommandsProvider(mockDeveloperIdProvider, mockResources, savedSearchesPage, persistentDataManager, savedSearchesMediator, searchPageFactory);

            var testSearchString = "is:issue author:testuser";
            var testSearchName = "My regular search";
            var jsonPayload = CreateJsonPayload(testSearchString, testSearchName, false);
            addSearchForm.SubmitForm(jsonPayload, string.Empty);

            await Task.Delay(DEFAULTTESTDELAYLONGMS);

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

            var savedItems = savedSearchesPage.GetItems();
            Assert.IsTrue(savedItems.Length == 2, "Should have our saved search and the add item");
            Assert.IsTrue(savedItems.Any(item => string.Equals(item.Title, testSearchName, StringComparison.Ordinal)));
            Assert.IsTrue(savedItems.Any(item => string.Equals(item.Title, "Add Saved Search", StringComparison.Ordinal)));

            var editSearchForm = new SaveSearchForm(savedSearch, persistentDataManager, mockResources, savedSearchesMediator);
            var editSearchString = "is:issue author:testuser";
            var editSearchName = "My Regular Search - Now top level";
            var editJsonPayload = CreateJsonPayload(editSearchString, editSearchName, true);

            editSearchForm.SubmitForm(editJsonPayload, string.Empty);

            await Task.Delay(DEFAULTTESTDELAYMS);

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

            var topLevelCommandsInCommandsProvider = commandsProvider.TopLevelCommands();
            Assert.IsTrue(topLevelCommandsInCommandsProvider.Any(c => string.Equals(c.Title, editSearchName, StringComparison.Ordinal)));
        }
        finally
        {
            persistentDataManager.Dispose();
            PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
        }
    }

    [TestMethod]
    public async Task Integration_RemoveTopLevelCommand_FromSavedSearches()
    {
        var (persistentDataManager, mockResources, savedSearchesMediator, dataStoreOptions) = CreateTestContext();
        try
        {
            var mockDeveloperIdProvider = CreateMockDeveloperIdProvider();
            var mockCacheDataManager = new Mock<ICacheDataManager>().Object;
            var searchPageFactory = new SearchPageFactory(mockCacheDataManager, persistentDataManager, mockResources, savedSearchesMediator);

            var addSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

            var mockAddSearchListItem = CreateMockAddSearchListItem();
            var savedSearchesPage = new SavedSearchesPage(searchPageFactory, persistentDataManager, mockResources, mockAddSearchListItem, savedSearchesMediator);

            var commandsProvider = CreateGitHubExtensionCommandsProvider(mockDeveloperIdProvider, mockResources, savedSearchesPage, persistentDataManager, savedSearchesMediator, searchPageFactory);

            var testSearchString = "is:issue author:testuser";
            var testSearchName = "Top level search";

            addSearchForm.SubmitForm(CreateJsonPayload(testSearchString, testSearchName, true), string.Empty);

            await Task.Delay(DEFAULTTESTDELAYLONGMS);

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
                    string.Equals(s.SearchString, testSearchString, StringComparison.Ordinal)),
                "Search should be in top level searches initially");

            var savedSearchesItems = savedSearchesPage.GetItems();
            Assert.IsTrue(savedSearchesItems.Length == 2, "Should have our saved search and the add item");
            Assert.IsTrue(savedSearchesItems.Any(item => string.Equals(item.Title, testSearchName, StringComparison.Ordinal)));
            Assert.IsTrue(savedSearchesItems.Any(item => string.Equals(item.Title, "Add Saved Search", StringComparison.Ordinal)));

            var topLevelCommands = commandsProvider.TopLevelCommands();
            Assert.IsTrue(topLevelCommands.Any(c => string.Equals(c.Title, testSearchName, StringComparison.Ordinal)));

            var topLevelSearch = initialTopLevelSearches.First(s =>
                string.Equals(s.Name, testSearchName, StringComparison.Ordinal) &&
                string.Equals(s.SearchString, testSearchString, StringComparison.Ordinal));
            var removeCommand = new RemoveSavedSearchCommand(topLevelSearch, persistentDataManager, mockResources, savedSearchesMediator);
            removeCommand.Invoke();

            await Task.Delay(DEFAULTTESTDELAYLONGMS);

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

            var updatedSavedSearchesItems = savedSearchesPage.GetItems();
            Assert.IsTrue(updatedSavedSearchesItems.Length == 1, "Should only have the add item");
            Assert.IsFalse(updatedSavedSearchesItems.Any(item => string.Equals(item.Title, testSearchName, StringComparison.Ordinal)));

            var updatedTopLevelCommands = commandsProvider.TopLevelCommands();
            Assert.IsFalse(updatedTopLevelCommands.Any(c => string.Equals(c.Title, testSearchName, StringComparison.Ordinal)));
        }
        finally
        {
            persistentDataManager.Dispose();
            PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
        }
    }

    [TestMethod]
    public async Task Integration_EditTopLevelSearch_ToBeNonTopLevel()
    {
        var (persistentDataManager, mockResources, savedSearchesMediator, dataStoreOptions) = CreateTestContext();
        try
        {
            var mockDeveloperIdProvider = CreateMockDeveloperIdProvider();
            var mockCacheDataManager = new Mock<ICacheDataManager>().Object;
            var searchPageFactory = new SearchPageFactory(mockCacheDataManager, persistentDataManager, mockResources, savedSearchesMediator);

            var addSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

            var mockAddSearchListItem = CreateMockAddSearchListItem();
            var savedSearchesPage = new SavedSearchesPage(searchPageFactory, persistentDataManager, mockResources, mockAddSearchListItem, savedSearchesMediator);

            var commandsProvider = CreateGitHubExtensionCommandsProvider(mockDeveloperIdProvider, mockResources, savedSearchesPage, persistentDataManager, savedSearchesMediator, searchPageFactory);

            var testSearchString = "is:issue author:testuser";
            var testSearchName = "Top level search";
            var testSearch = new SearchCandidate(testSearchString, testSearchName, true);
            await persistentDataManager.UpdateSearchTopLevelStatus(testSearch, testSearch.IsTopLevel);
            savedSearchesMediator.AddSearch(testSearch);

            await Task.Delay(DEFAULTTESTDELAYLONGMS);

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

            var editSearchForm = new SaveSearchForm(testSearch, persistentDataManager, mockResources, savedSearchesMediator);

            var editJsonPayload = CreateJsonPayload(testSearchString, testSearchName, false);

            editSearchForm.SubmitForm(editJsonPayload, string.Empty);

            await Task.Delay(DEFAULTTESTDELAYMS);

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

            var updatedSavedSearchPageItems = savedSearchesPage.GetItems();
            Assert.IsTrue(updatedSavedSearchPageItems.Length == 2, "Should have our saved search and the add item");
            Assert.IsTrue(updatedSavedSearchPageItems.Any(item => string.Equals(item.Title, testSearchName, StringComparison.Ordinal)));
            Assert.IsTrue(updatedSavedSearchPageItems.Any(item => string.Equals(item.Title, "Add Saved Search", StringComparison.Ordinal)));

            var updatedTopLevelCommands = commandsProvider.TopLevelCommands();
            Assert.IsFalse(updatedTopLevelCommands.Any(c => string.Equals(c.Title, testSearchName, StringComparison.Ordinal)));
        }
        finally
        {
            persistentDataManager.Dispose();
            PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
        }
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
