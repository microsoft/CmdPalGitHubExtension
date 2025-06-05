// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.Controls.Forms;
using GitHubExtension.Controls.Pages;
using GitHubExtension.DataModel;
using GitHubExtension.Helpers;
using GitHubExtension.PersistentData;
using GitHubExtension.Test.Helpers;
using GitHubExtension.Test.PersistentData;
using Moq;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class TopLevelSearchesTest
{
    private (PersistentDataManager PersistentDataManager, IResources Resources, SavedSearchesMediator Mediator, DataStoreOptions DataStoreOptions) CreateTestContext()
    {
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        var persistentDataManager = TestHelpers.CreatePersistentDataManager(dataStoreOptions);
        var resources = new Mock<IResources>().Object;
        var mediator = new SavedSearchesMediator();
        return (persistentDataManager, resources, mediator, dataStoreOptions);
    }

    [DataRow("is:issue author:testuser", "Test Search", true, true)]
    [DataRow("is:pr author:testuser", "Test Search", false, false)]
    [TestMethod]
    public async Task SaveSearchForm_TopLevelStateIsCorrect(string searchString, string searchName, bool initialTopLevel, bool expectedTopLevel)
    {
        var (persistentDataManager, resources, mediator, dataStoreOptions) = CreateTestContext();
        try
        {
            var saveSearchForm = new SaveSearchForm(persistentDataManager, resources, mediator);
            var jsonPayload = TestHelpers.CreateJsonPayload(searchString, searchName, initialTopLevel);

            var tcs = TestHelpers.CreateTaskCompletionSource(mediator);
            saveSearchForm.SubmitForm(jsonPayload, string.Empty);
            await tcs.Task;

            var savedSearches = await persistentDataManager.GetSavedSearches();
            Assert.IsTrue(savedSearches.Any(s => string.Equals(s.Name, searchName, StringComparison.Ordinal)), "The new search should appear in saved searches");

            var editSearchForm = new SaveSearchForm(savedSearches.First(), persistentDataManager, resources, mediator);
            Assert.AreEqual(expectedTopLevel, await editSearchForm.GetIsTopLevel());
        }
        catch (Exception ex)
        {
            // No errors expected
            Assert.Fail(ex.Message);
        }
        finally
        {
            persistentDataManager.Dispose();
            PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
        }
    }

    [DataRow(true, false)]
    [DataRow(false, true)]
    [TestMethod]
    public async Task SaveSearchForm_ChangeTopLevelState(bool initialTopLevel, bool newTopLevel)
    {
        var (persistentDataManager, resources, mediator, dataStoreOptions) = CreateTestContext();
        try
        {
            var searchString = "dummy search";
            var searchName = "Dummy Search";
            var dummySearch = new SearchCandidate(searchString, searchName, initialTopLevel);

            await persistentDataManager.UpdateSearchTopLevelStatus(dummySearch, initialTopLevel);
            mediator.AddSearch(dummySearch);

            var saveSearchForm = new SaveSearchForm(dummySearch, persistentDataManager, resources, mediator);

            var jsonPayload = TestHelpers.CreateJsonPayload(searchString, searchName, newTopLevel);

            var tcs = TestHelpers.CreateTaskCompletionSource(mediator);
            saveSearchForm.SubmitForm(jsonPayload, string.Empty);
            await tcs.Task;

            var editSearchForm = new SaveSearchForm(dummySearch, persistentDataManager, resources, mediator);
            Assert.AreEqual(newTopLevel, await editSearchForm.GetIsTopLevel());

            var updatedTopLevelSearches = await persistentDataManager.GetTopLevelSearches();
            Assert.AreEqual(newTopLevel, updatedTopLevelSearches.Any(s => s.Name == searchName && s.SearchString == searchString));
        }
        catch (Exception ex)
        {
            // No errors expected
            Assert.Fail(ex.Message);
        }
        finally
        {
            persistentDataManager.Dispose();
            PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
        }
    }

    [DataRow("is:issue author:testuser", "New Top Level Search", true)]
    [DataRow("is:pr author:testuser", "Another Top Level Search", true)]
    [TestMethod]
    public async Task Integration_AddNewTopLevelCommand(string searchString, string searchName, bool isTopLevel)
    {
        var (persistentDataManager, resources, mediator, dataStoreOptions) = CreateTestContext();
        try
        {
            var mockDeveloperIdProvider = TestHelpers.CreateMockDeveloperIdProvider();
            var mockCacheDataManager = new Mock<ICacheDataManager>().Object;
            var searchPageFactory = new SearchPageFactory(mockCacheDataManager, persistentDataManager, resources, mediator);

            var addSearchForm = new SaveSearchForm(persistentDataManager, resources, mediator);

            var mockAddSearchListItem = TestHelpers.CreateMockAddSearchListItem();
            var savedSearchesPage = new SavedSearchesPage(searchPageFactory, persistentDataManager, resources, mockAddSearchListItem, mediator);

            var commandsProvider = TestHelpers.CreateGitHubExtensionCommandsProvider(mockDeveloperIdProvider, resources, savedSearchesPage, persistentDataManager, mediator, searchPageFactory);

            var jsonPayload = TestHelpers.CreateJsonPayload(searchString, searchName, isTopLevel);

            var tcs = TestHelpers.CreateTaskCompletionSource(mediator);
            addSearchForm.SubmitForm(jsonPayload, string.Empty);
            await tcs.Task;

            var savedSearches = await persistentDataManager.GetSavedSearches();
            Assert.IsTrue(
                savedSearches.Any(s =>
                string.Equals(s.Name, searchName, StringComparison.Ordinal) &&
                string.Equals(s.SearchString, searchString, StringComparison.Ordinal)),
                "The new search should appear in saved searches");

            var persitentDataManagerTopLevelCommands = await persistentDataManager.GetTopLevelSearches();
            Assert.IsTrue(
                persitentDataManagerTopLevelCommands.Any(s =>
                string.Equals(s.Name, searchName, StringComparison.Ordinal) &&
                string.Equals(s.SearchString, searchString, StringComparison.Ordinal)),
                "The new search should appear in top level commands");

            var savedSearchesItems = savedSearchesPage.GetItems();
            Assert.IsTrue(savedSearchesItems.Length == 2, "Should have our saved search and the add item");
            Assert.IsTrue(savedSearchesItems.Any(item => string.Equals(item.Title, searchName, StringComparison.Ordinal)));

            var topLevelCommands = commandsProvider.TopLevelCommands();
            Assert.IsTrue(topLevelCommands.Any(c => string.Equals(c.Title, searchName, StringComparison.Ordinal)));
        }
        catch (Exception ex)
        {
            // No errors expected
            Assert.Fail(ex.Message);
        }
        finally
        {
            persistentDataManager.Dispose();
            PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
        }
    }
}
