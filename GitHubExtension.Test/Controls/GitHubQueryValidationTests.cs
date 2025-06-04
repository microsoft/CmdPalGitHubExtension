// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.Controls.Forms;
using GitHubExtension.DataModel;
using GitHubExtension.Helpers;
using GitHubExtension.PersistentData;
using GitHubExtension.Test.PersistentData;
using Moq;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class GitHubQueryValidationTests
{
    public SaveSearchForm CreateSaveSearchForm(PersistentDataManager persistentDataManager)
    {
        var mockResources = new Mock<IResources>();
        var savedSearchesMediator = new SavedSearchesMediator();
        return new SaveSearchForm(persistentDataManager, mockResources.Object, savedSearchesMediator);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsIsOpenKeyword()
    {
        // Initialize
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var testSearchString = "is:open";
        var payload = CreatePayload(testSearchString, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, testSearchString, StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsIsIssueKeyword()
    {
        // Initialize
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var testSearchString = "is:issue";
        var payload = CreatePayload(testSearchString, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, testSearchString, StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsIsPullRequestKeyword()
    {
        // Initialize
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var testSearchString = "is:pr";
        var payload = CreatePayload(testSearchString, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, testSearchString, StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsMultipleKeywords()
    {
        // Initialize
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var testSearchString = "is:open is:issue";
        var payload = CreatePayload(testSearchString, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, testSearchString, StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsRepoQualifier()
    {
        // Initialize
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var testSearchString = "is:issue repo:microsoft/PowerToys";
        var payload = CreatePayload(testSearchString, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, testSearchString, StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsAuthorQualifier()
    {
        // Initialize
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var testSearchString = "is:pr author:octocat";
        var payload = CreatePayload(testSearchString, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, testSearchString, StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsStateAndLabelQualifiers()
    {
        // Initialize
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var testSearchString = "state:open label:bug";
        var payload = CreatePayload(testSearchString, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, testSearchString, StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsInvolvesAndLanguageQualifiers()
    {
        // Initialize
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var testSearchString = "involves:defunkt language:javascript";
        var payload = CreatePayload(testSearchString, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, testSearchString, StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsOrgAndCreatedQualifiers()
    {
        // Initialize
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var testSearchString = "org:github created:>2022-01-01";
        var payload = CreatePayload(testSearchString, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, testSearchString, StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsAssigneeAndMilestoneQualifiers()
    {
        // Initialize
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var testSearchString = "is:issue assignee:@me milestone:v1.0";
        var payload = CreatePayload(testSearchString, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, testSearchString, StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsReviewQualifier()
    {
        // Initialize
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var testSearchString = "is:pr review:approved";
        var payload = CreatePayload(testSearchString, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, testSearchString, StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsInQualifier()
    {
        // Initialize
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var testSearchString = "is:issue in:title error";
        var payload = CreatePayload(testSearchString, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, testSearchString, StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsMergedDateQualifier()
    {
        // Initialize
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var testSearchString = "is:pr merged:>=2023-01-01";
        var payload = CreatePayload(testSearchString, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, testSearchString, StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsSingleLabelQualifier()
    {
        // Initialize
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var testSearchString = "label:enhancement";
        var payload = CreatePayload(testSearchString, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, testSearchString, StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsMultipleLabelQualifiers()
    {
        // Initialize
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var testSearchString = "label:bug label:help-wanted label:documentation";
        var payload = CreatePayload(testSearchString, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, testSearchString, StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsExcludingLabel()
    {
        // Initialize
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions(); // created here because we dispose dataStoreOptions at the end of this test
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var testSearchString = "-label:wontfix";
        var payload = CreatePayload(testSearchString, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        var search = new SearchCandidate(testSearchString, "Test Search");

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, testSearchString, StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsExcludingMultipleLabels()
    {
        // Initialize
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var testSearchString = "-label:wontfix -label:duplicate -label:invalid";
        var payload = CreatePayload(testSearchString, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, testSearchString, StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsExcludingOtherQualifiers()
    {
        // Initialize
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var testSearchString = "is:issue -is:closed -author:bot";
        var payload = CreatePayload(testSearchString, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, testSearchString, StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    // this is a valid search that returns no results
    [TestMethod]
    public async Task ValidateSearch_SupportsMixOfIncludeAndExcludeQualifiers()
    {
        // Initialize
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var testSearchString = "is:pr label:enhancement -label:wontfix repo:microsoft/PowerToys -is:draft";
        var payload = CreatePayload(testSearchString, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, testSearchString, StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    // this is a valid search, but returned no results
    [TestMethod]
    public async Task ValidateSearch_SupportsBooleanOperators()
    {
        // Initialize
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var testSearchString = "is:open AND (is:issue OR is:pr) NOT author:bot devhome";
        var payload = CreatePayload(testSearchString, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, testSearchString, StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsMultipleRepositories()
    {
        // Initialize
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var testSearchString = "repo:microsoft/terminal repo:microsoft/PowerToys repo:microsoft/vscode is:open is:issue";
        var payload = CreatePayload(testSearchString, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, testSearchString, StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateURL_WithRepositoryIssuesUrl_ParsesAndReturnsFormattedSearchString()
    {
        // Initialize
        var url = "https://github.com/microsoft/PowerToys/issues?q=is:open+label:bug";
        var expected = "repo:microsoft/PowerToys is:open label:bug";
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var payload = CreatePayload(url, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, expected, StringComparison.OrdinalIgnoreCase) && string.Equals(s.Name, "Test Search", StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateURL_WithRepositoryIssuesWithoutQuery_ParsesAndReturnsRepoBasedSearchString()
    {
        // Initialize
        var url = "https://github.com/microsoft/PowerToys/issues";
        var expected = "repo:microsoft/PowerToys is:issue is:open";
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var payload = CreatePayload(url, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, expected, StringComparison.OrdinalIgnoreCase) && string.Equals(s.Name, "Test Search", StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateURL_WithRepositoryClosedIssuesUrl_ParsesAndReturnsClosedIssuesSearchString()
    {
        // Initialize
        var url = "https://github.com/microsoft/PowerToys/issues?q=is:issue+is:closed";
        var expected = "repo:microsoft/PowerToys is:issue is:closed";
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var payload = CreatePayload(url, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, expected, StringComparison.OrdinalIgnoreCase) && string.Equals(s.Name, "Test Search", StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateURL_WithPullRequestsUrl_ParsesAndReturnsPrSearchString()
    {
        // Initialize
        var url = "https://github.com/microsoft/PowerToys/pulls";
        var expected = "repo:microsoft/PowerToys is:pr is:open";
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var payload = CreatePayload(url, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, expected, StringComparison.OrdinalIgnoreCase) && string.Equals(s.Name, "Test Search", StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateURL_WithSearchPagesUrl_ParsesAndReturnsBasicSearchString()
    {
        // Initialize
        var url = "https://github.com/search/issues";
        var expected = "is:issue";
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var payload = CreatePayload(url, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, expected, StringComparison.OrdinalIgnoreCase) && string.Equals(s.Name, "Test Search", StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateURL_WithInvalidUrl_UsesOriginalString()
    {
        // Initialize
        var url = "not a url";
        var expected = "not a url";
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var payload = CreatePayload(url, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, expected, StringComparison.OrdinalIgnoreCase) && string.Equals(s.Name, "Test Search", StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateURL_WithEmptyUrl_ReturnsEmptySearchString()
    {
        // Initialize
        var url = "   ";
        var expected = "   ";
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var payload = CreatePayload(url, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, expected, StringComparison.OrdinalIgnoreCase) && string.Equals(s.Name, "Test Search", StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateURL_WithMultipleQualifiers_ParsesAndPreservesAllQualifiers()
    {
        // Initialize
        var url = "https://github.com/search?q=repo:microsoft/PowerToys+is:open+is:issue+label:bug+author:octocat";
        var expected = "repo:microsoft/PowerToys is:open is:issue label:bug author:octocat";
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var payload = CreatePayload(url, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, expected, StringComparison.OrdinalIgnoreCase) && string.Equals(s.Name, "Test Search", StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateURL_WithNegatedQualifiers_ParsesAndPreservesNegation()
    {
        // Initialize
        var url = "https://github.com/search?q=repo:microsoft/PowerToys+is:open+-label:wontfix";
        var expected = "repo:microsoft/PowerToys is:open -label:wontfix";
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var payload = CreatePayload(url, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, expected, StringComparison.OrdinalIgnoreCase) && string.Equals(s.Name, "Test Search", StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateURL_WithMultipleRepositories_ParsesAndReturnsCorrectSearchString()
    {
        // Initialize
        var url = "https://github.com/search?q=repo:microsoft/terminal+repo:microsoft/PowerToys+repo:microsoft/vscode+is:open+is:issue";
        var expected = "repo:microsoft/terminal repo:microsoft/PowerToys repo:microsoft/vscode is:open is:issue";
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var payload = CreatePayload(url, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, expected, StringComparison.OrdinalIgnoreCase) && string.Equals(s.Name, "Test Search", StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateURL_WithMultipleStates_ParsesAndReturnsCorrectSearchString()
    {
        // Initialize
        var url = "https://github.com/search?q=repo:microsoft/PowerToys+state:open+state:closed";
        var expected = "repo:microsoft/PowerToys state:open state:closed";
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var payload = CreatePayload(url, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, expected, StringComparison.OrdinalIgnoreCase) && string.Equals(s.Name, "Test Search", StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateURL_WithMultipleSortDirections_ParsesAndReturnsCorrectSearchString()
    {
        // Initialize
        var url = "https://github.com/search?q=repo:microsoft/PowerToys+sort:updated-desc+sort:created-asc";
        var expected = "repo:microsoft/PowerToys sort:updated-desc sort:created-asc";
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var payload = CreatePayload(url, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, expected, StringComparison.OrdinalIgnoreCase) && string.Equals(s.Name, "Test Search", StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    public async Task ValidateURL_WithMultipleLanguagesMilestonesDates_ParsesAndReturnsCorrectSearchString()
    {
        // Initialize
        var url = "https://github.com/search?q=repo:microsoft/PowerToys+language:csharp+language:javascript+milestone:v1.0+milestone:v2.0+created:>2022-01-01+updated:<2023-01-01";
        var expected = "repo:microsoft/PowerToys language:csharp language:javascript milestone:v1.0 milestone:v2.0 created:>2022-01-01 updated:<2023-01-01";
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var persistentDataManager = CreatePersistentDataManager(dataStoreOptions);
        var mockResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(persistentDataManager, mockResources, savedSearchesMediator);

        // Create search string and payload
        var payload = CreatePayload(url, "Test Search");
        saveSearchForm.SubmitForm(payload, string.Empty);

        await Task.Delay(100); // Simulate async operation

        // Validate the search
        var searches = await persistentDataManager.GetSavedSearches();
        Assert.IsTrue(searches.Count() == 1);
        Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, expected, StringComparison.OrdinalIgnoreCase) && string.Equals(s.Name, "Test Search", StringComparison.OrdinalIgnoreCase)));

        // Clean up
        persistentDataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    private string CreatePayload(string searchString, string name)
    {
        return $"{{ \"EnteredSearch\": \"{searchString}\", \"Name\": \"{name}\", \"IsTopLevel\": \"false\" }}";
    }

    public PersistentDataManager CreatePersistentDataManager(DataStoreOptions dataStoreOptions)
    {
        var stubValidator = new Mock<IGitHubValidator>().Object;
        return new PersistentDataManager(stubValidator, dataStoreOptions);
    }
}
