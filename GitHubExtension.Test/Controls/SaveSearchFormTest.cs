// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using System.Text.Json.Nodes;
using GitHubExtension.Controls;
using GitHubExtension.Controls.Forms;
using GitHubExtension.Helpers;
using Moq;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class SaveSearchFormTest
{
    [TestMethod]
    public void CreateSearchFromJson_ShouldReturnCorrectSearchCandidate()
    {
        var jsonPayload = JsonNode.Parse(@"
        {
            ""EnteredSearch"": ""author:username"",
            ""Name"": ""My Combined Search"",
            ""IsTopLevel"": ""true""
        }");

        var searchCandidate = SaveSearchForm.CreateSearchFromJson(jsonPayload);

        Assert.IsNotNull(searchCandidate);
        Assert.AreEqual("author:username", searchCandidate.SearchString);
        Assert.AreEqual("My Combined Search", searchCandidate.Name);
        Assert.IsTrue(searchCandidate.IsTopLevel);
    }

    [TestMethod]
    public void SubmitForm_ShouldSaveIssueSearch_WhenIssueSearchIsProvided()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository
            .Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        ISearch? capturedSearch = null;
        mockSearchRepository
            .Setup(repo => repo.UpdateSearchTopLevelStatus(It.IsAny<SearchCandidate>(), false))
            .Callback<ISearch, bool>((s, isTopLevel) => capturedSearch = s)
            .Returns(Task.CompletedTask);

        mockSearchRepository
            .Setup(repo => repo.RemoveSavedSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        var stubResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, stubResources, savedSearchesMediator);

        var jsonPayload = JsonNode.Parse(@"
            {
                ""EnteredSearch"": ""is:issue author:username"",
                ""Name"": ""My Issue Search"",
                ""IsTopLevel"": ""false""
            }")?.ToString();

        saveSearchForm.SubmitForm(jsonPayload, string.Empty);

        Thread.Sleep(100);

        Assert.IsNotNull(capturedSearch);
        Assert.AreEqual("is:issue author:username", capturedSearch.SearchString);
        Assert.AreEqual("My Issue Search", capturedSearch.Name);
    }

    [TestMethod]
    public void SubmitForm_ShouldSavePullRequestSearch_WhenPRSearchIsProvided()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository
            .Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        ISearch? capturedSearch = null;
        mockSearchRepository
            .Setup(repo => repo.UpdateSearchTopLevelStatus(It.IsAny<SearchCandidate>(), false))
            .Callback<ISearch, bool>((s, isTopLevel) => capturedSearch = s)
            .Returns(Task.CompletedTask);

        mockSearchRepository
            .Setup(repo => repo.RemoveSavedSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        var stubResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, stubResources, savedSearchesMediator);

        var jsonPayload = JsonNode.Parse(@"
            {
                ""EnteredSearch"": ""is:pr author:username"",
                ""Name"": ""My PR Search"",
                ""IsTopLevel"": ""false""
            }")?.ToString();

        saveSearchForm.SubmitForm(jsonPayload, string.Empty);

        Thread.Sleep(100);

        Assert.IsNotNull(capturedSearch);
        Assert.AreEqual("is:pr author:username", capturedSearch.SearchString);
        Assert.AreEqual("My PR Search", capturedSearch.Name);
    }

    [TestMethod]
    public void SubmitForm_ShouldSaveCombinedSearch_WhenNoTypeIsProvided()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository
            .Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        ISearch? capturedSearch = null;
        mockSearchRepository
            .Setup(repo => repo.UpdateSearchTopLevelStatus(It.IsAny<SearchCandidate>(), false))
            .Callback<ISearch, bool>((s, isTopLevel) => capturedSearch = s)
            .Returns(Task.CompletedTask);

        mockSearchRepository
            .Setup(repo => repo.RemoveSavedSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        var stubResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, stubResources, savedSearchesMediator);

        var jsonPayload = JsonNode.Parse(@"
            {
                ""EnteredSearch"": ""author:username"",
                ""Name"": ""My Combined Search"",
                ""IsTopLevel"": ""false""
            }")?.ToString();

        saveSearchForm.SubmitForm(jsonPayload, string.Empty);

        Thread.Sleep(100);

        Assert.IsNotNull(capturedSearch);
        Assert.AreEqual("author:username", capturedSearch.SearchString);
        Assert.AreEqual("My Combined Search", capturedSearch.Name);
    }

    [TestMethod]
    public void SubmitForm_ShouldEditSearchString_WhenUpdatingExistingSearch()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository
            .Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);
        mockSearchRepository
            .Setup(repo => repo.UpdateSearchTopLevelStatus(It.IsAny<ISearch>(), false))
            .Returns(Task.CompletedTask);
        mockSearchRepository
            .Setup(repo => repo.RemoveSavedSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        var existingSearch = new SearchCandidate("old search", "My Search", false);
        var stubResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(existingSearch, mockSearchRepository.Object, stubResources, savedSearchesMediator);

        var jsonPayload = JsonNode.Parse(@"
            {
                ""EnteredSearch"": ""new search"",
                ""Name"": ""My Search"",
                ""IsTopLevel"": ""false""
            }")?.ToString();

        saveSearchForm.SubmitForm(jsonPayload, string.Empty);

        Thread.Sleep(100);

        mockSearchRepository.Verify(
            repo =>
            repo.UpdateSearchTopLevelStatus(
                It.Is<SearchCandidate>(s =>
                s.SearchString == "new search" &&
                s.Name == "My Search"),
                false),
            Times.Once);

        mockSearchRepository.Verify(
            repo =>
            repo.RemoveSavedSearch(It.Is<ISearch>(s =>
                s.SearchString == "old search" &&
                s.Name == "My Search")),
            Times.Once);
    }

    [TestMethod]
    public void SubmitForm_ShouldEditSearchName_WhenUpdatingExistingSearch()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository
            .Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);
        mockSearchRepository
            .Setup(repo => repo.UpdateSearchTopLevelStatus(It.IsAny<ISearch>(), false))
            .Returns(Task.CompletedTask);
        mockSearchRepository
            .Setup(repo => repo.RemoveSavedSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        var existingSearch = new SearchCandidate("my search", "Old Name", false);
        var stubResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(existingSearch, mockSearchRepository.Object, stubResources, savedSearchesMediator);

        var jsonPayload = JsonNode.Parse(@"
            {
                ""EnteredSearch"": ""my search"",
                ""Name"": ""New Name"",
                ""IsTopLevel"": ""false""
            }")?.ToString();

        saveSearchForm.SubmitForm(jsonPayload, string.Empty);

        Thread.Sleep(100);

        mockSearchRepository.Verify(
            repo =>
            repo.UpdateSearchTopLevelStatus(
                It.Is<SearchCandidate>(s =>
                s.SearchString == "my search" &&
                s.Name == "New Name"),
                false),
            Times.Once);

        mockSearchRepository.Verify(
            repo =>
            repo.RemoveSavedSearch(It.Is<ISearch>(s =>
                s.SearchString == "my search" &&
                s.Name == "Old Name")),
            Times.Once);
    }

    [TestMethod]
    public void SubmitForm_ShouldEditBothNameAndString_WhenUpdatingExistingSearch()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository
            .Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);
        mockSearchRepository
            .Setup(repo => repo.UpdateSearchTopLevelStatus(It.IsAny<ISearch>(), false))
            .Returns(Task.CompletedTask);
        mockSearchRepository
            .Setup(repo => repo.RemoveSavedSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        var existingSearch = new SearchCandidate("old search", "Old Name", false);
        var stubResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(existingSearch, mockSearchRepository.Object, stubResources, savedSearchesMediator);

        var jsonPayload = JsonNode.Parse(@"
            {
                ""EnteredSearch"": ""new search"",
                ""Name"": ""New Name"",
                ""IsTopLevel"": ""false""
            }")?.ToString();

        saveSearchForm.SubmitForm(jsonPayload, string.Empty);

        Thread.Sleep(100);

        mockSearchRepository.Verify(
            repo =>
            repo.UpdateSearchTopLevelStatus(
                It.Is<SearchCandidate>(s =>
                s.SearchString == "new search" &&
                s.Name == "New Name"),
                false),
            Times.Once);

        mockSearchRepository.Verify(
            repo =>
            repo.RemoveSavedSearch(It.Is<ISearch>(s =>
                s.SearchString == "old search" &&
                s.Name == "Old Name")),
            Times.Once);
    }

    [TestMethod]
    public void SubmitForm_ShouldOnlyUpdateTopLevel_WhenNothingElseChanges()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository
            .Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);
        mockSearchRepository
            .Setup(repo => repo.UpdateSearchTopLevelStatus(It.IsAny<ISearch>(), false))
            .Returns(Task.CompletedTask);
        mockSearchRepository
            .Setup(repo => repo.RemoveSavedSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        var existingSearch = new SearchCandidate("my search", "My Search", false);
        var stubResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(existingSearch, mockSearchRepository.Object, stubResources, savedSearchesMediator);

        var jsonPayload = JsonNode.Parse(@"
            {
                ""EnteredSearch"": ""my search"",
                ""Name"": ""My Search"",
                ""IsTopLevel"": ""true""
            }")?.ToString();

        saveSearchForm.SubmitForm(jsonPayload, string.Empty);

        Thread.Sleep(100);

        mockSearchRepository.Verify(
            repo =>
            repo.UpdateSearchTopLevelStatus(
                It.Is<SearchCandidate>(s =>
                s.SearchString == "my search" &&
                s.Name == "My Search" &&
                s.IsTopLevel == true),
                true),
            Times.Once);

        mockSearchRepository.Verify(
            repo =>
            repo.RemoveSavedSearch(It.Is<ISearch>(s =>
                s.SearchString == "my search" &&
                s.Name == "My Search")),
            Times.Once);
    }

    [TestMethod]
    public async Task GetSearchAsync_WithGitHubQueryUrl_ParsesAndReturnsSearchString()
    {
        var url = "https://github.com/search?q=repo%3Amicrosoft%2FPowerToys+is%3Aissue+label%3Abug&type=issues";
        var expected = "repo:microsoft/PowerToys is:issue label:bug";

        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);
        mockSearchRepository.Setup(repo => repo.UpdateSearchTopLevelStatus(It.IsAny<ISearch>(), It.IsAny<bool>())).Returns(Task.CompletedTask);
        mockSearchRepository.Setup(repo => repo.IsTopLevel(It.IsAny<ISearch>())).ReturnsAsync(false);

        var mockResources = new Mock<IResources>();

        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object, savedSearchesMediator);
        var payload = CreatePayload(url, "Test Search");

        var result = await saveSearchForm.GetSearchAsync(payload);

        Assert.AreEqual(expected, result.SearchString);
        Assert.AreEqual("Test Search", result.Name);
        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == expected)), Times.Once);
    }

    [TestMethod]
    public async Task GetSearchAsync_WithRepositoryIssuesUrl_ParsesAndReturnsFormattedSearchString()
    {
        var url = "https://github.com/microsoft/PowerToys/issues?q=is:open+label:bug";
        var expected = "repo:microsoft/PowerToys is:open label:bug";

        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);
        mockSearchRepository.Setup(repo => repo.UpdateSearchTopLevelStatus(It.IsAny<ISearch>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

        var mockResources = new Mock<IResources>();

        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object, savedSearchesMediator);
        var payload = CreatePayload(url, "Test Search");

        var result = await saveSearchForm.GetSearchAsync(payload);

        Assert.AreEqual(expected, result.SearchString);
        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == expected)), Times.Once);
    }

    [TestMethod]
    public async Task GetSearchAsync_WithRepositoryIssuesWithoutQuery_ParsesAndReturnsRepoBasedSearchString()
    {
        var url = "https://github.com/microsoft/PowerToys/issues";
        var expected = "repo:microsoft/PowerToys is:issue is:open";

        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);
        mockSearchRepository.Setup(repo => repo.UpdateSearchTopLevelStatus(It.IsAny<ISearch>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

        var mockResources = new Mock<IResources>();

        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object, savedSearchesMediator);
        var payload = CreatePayload(url, "Test Search");

        var result = await saveSearchForm.GetSearchAsync(payload);

        Assert.AreEqual(expected, result.SearchString);
        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == expected)), Times.Once);
    }

    [TestMethod]
    public async Task GetSearchAsync_WithRepositoryClosedIssuesUrl_ParsesAndReturnsClosedIssuesSearchString()
    {
        var url = "https://github.com/microsoft/PowerToys/issues?q=is:issue+is:closed";
        var expected = "repo:microsoft/PowerToys is:issue is:closed";

        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);
        mockSearchRepository.Setup(repo => repo.UpdateSearchTopLevelStatus(It.IsAny<ISearch>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

        var mockResources = new Mock<IResources>();

        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object, savedSearchesMediator);
        var payload = CreatePayload(url, "Test Search");

        var result = await saveSearchForm.GetSearchAsync(payload);

        Assert.AreEqual(expected, result.SearchString);
        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == expected)), Times.Once);
    }

    [TestMethod]
    public async Task GetSearchAsync_WithPullRequestsUrl_ParsesAndReturnsPrSearchString()
    {
        var url = "https://github.com/microsoft/PowerToys/pulls";
        var expected = "repo:microsoft/PowerToys is:pr is:open";

        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);
        mockSearchRepository.Setup(repo => repo.UpdateSearchTopLevelStatus(It.IsAny<ISearch>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

        var mockResources = new Mock<IResources>();

        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object, savedSearchesMediator);
        var payload = CreatePayload(url, "Test Search");

        var result = await saveSearchForm.GetSearchAsync(payload);

        Assert.AreEqual(expected, result.SearchString);
        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == expected)), Times.Once);
    }

    [TestMethod]
    public async Task GetSearchAsync_WithSearchPagesUrl_ParsesAndReturnsBasicSearchString()
    {
        var url = "https://github.com/search/issues";
        var expected = "is:issue";

        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);
        mockSearchRepository.Setup(repo => repo.UpdateSearchTopLevelStatus(It.IsAny<ISearch>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

        var mockResources = new Mock<IResources>();

        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object, savedSearchesMediator);
        var payload = CreatePayload(url, "Test Search");

        var result = await saveSearchForm.GetSearchAsync(payload);

        Assert.AreEqual(expected, result.SearchString);
        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == expected)), Times.Once);
    }

    [TestMethod]
    public async Task GetSearchAsync_WithInvalidUrl_UsesOriginalString()
    {
        var invalidUrl = "not a url";
        var expected = "not a url";

        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);
        mockSearchRepository.Setup(repo => repo.UpdateSearchTopLevelStatus(It.IsAny<ISearch>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

        var mockResources = new Mock<IResources>();

        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object, savedSearchesMediator);
        var payload = CreatePayload(invalidUrl, "Test Search");

        var result = await saveSearchForm.GetSearchAsync(payload);

        Assert.AreEqual(expected, result.SearchString);
        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == expected)), Times.Once);
    }

    [TestMethod]
    public async Task GetSearchAsync_WithEmptyUrl_ReturnsEmptySearchString()
    {
        var emptyUrl = "   ";
        var expected = "   ";

        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);
        mockSearchRepository.Setup(repo => repo.UpdateSearchTopLevelStatus(It.IsAny<ISearch>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

        var mockResources = new Mock<IResources>();

        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object, savedSearchesMediator);
        var payload = CreatePayload(emptyUrl, "Test Search");

        var result = await saveSearchForm.GetSearchAsync(payload);

        Assert.AreEqual(expected, result.SearchString);
        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == expected)), Times.Once);
    }

    [TestMethod]
    public async Task GetSearchAsync_WithMultipleQualifiers_ParsesAndPreservesAllQualifiers()
    {
        var url = "https://github.com/search?q=repo:microsoft/PowerToys+is:open+is:issue+label:bug+author:octocat";
        var expected = "repo:microsoft/PowerToys is:open is:issue label:bug author:octocat";

        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);
        mockSearchRepository.Setup(repo => repo.UpdateSearchTopLevelStatus(It.IsAny<ISearch>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

        var mockResources = new Mock<IResources>();

        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object, savedSearchesMediator);
        var payload = CreatePayload(url, "Test Search");

        var result = await saveSearchForm.GetSearchAsync(payload);

        Assert.AreEqual(expected, result.SearchString);
        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == expected)), Times.Once);
    }

    [TestMethod]
    public async Task GetSearchAsync_WithNegatedQualifiers_ParsesAndPreservesNegation()
    {
        var url = "https://github.com/search?q=repo:microsoft/PowerToys+is:open+-label:wontfix";
        var expected = "repo:microsoft/PowerToys is:open -label:wontfix";

        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);
        mockSearchRepository.Setup(repo => repo.UpdateSearchTopLevelStatus(It.IsAny<ISearch>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

        var mockResources = new Mock<IResources>();

        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object, savedSearchesMediator);
        var payload = CreatePayload(url, "Test Search");

        var result = await saveSearchForm.GetSearchAsync(payload);

        Assert.AreEqual(expected, result.SearchString);
        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == expected)), Times.Once);
    }

    [TestMethod]
    public async Task GetSearchAsync_WithMultipleRepositories_ParsesAndReturnsCorrectSearchString()
    {
        var url = "https://github.com/search?q=repo:microsoft/terminal+repo:microsoft/PowerToys+repo:microsoft/vscode+is:open+is:issue";
        var expected = "repo:microsoft/terminal repo:microsoft/PowerToys repo:microsoft/vscode is:open is:issue";

        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);
        mockSearchRepository.Setup(repo => repo.UpdateSearchTopLevelStatus(It.IsAny<ISearch>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

        var mockResources = new Mock<IResources>();

        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object, savedSearchesMediator);
        var payload = CreatePayload(url, "Test Search");

        var result = await saveSearchForm.GetSearchAsync(payload);

        Assert.AreEqual(expected, result.SearchString);
        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == expected)), Times.Once);
    }

    [TestMethod]
    public async Task GetSearchAsync_WithMultipleStates_ParsesAndReturnsCorrectSearchString()
    {
        var url = "https://github.com/search?q=repo:microsoft/PowerToys+state:open+state:closed";
        var expected = "repo:microsoft/PowerToys state:open state:closed";

        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);
        mockSearchRepository.Setup(repo => repo.UpdateSearchTopLevelStatus(It.IsAny<ISearch>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

        var mockResources = new Mock<IResources>();

        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object, savedSearchesMediator);
        var payload = CreatePayload(url, "Test Search");

        var result = await saveSearchForm.GetSearchAsync(payload);

        Assert.AreEqual(expected, result.SearchString);
        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == expected)), Times.Once);
    }

    [TestMethod]
    public async Task GetSearchAsync_WithMultipleSortDirections_ParsesAndReturnsCorrectSearchString()
    {
        var url = "https://github.com/search?q=repo:microsoft/PowerToys+sort:updated-desc+sort:created-asc";
        var expected = "repo:microsoft/PowerToys sort:updated-desc sort:created-asc";

        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);
        mockSearchRepository.Setup(repo => repo.UpdateSearchTopLevelStatus(It.IsAny<ISearch>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

        var mockResources = new Mock<IResources>();

        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object, savedSearchesMediator);
        var payload = CreatePayload(url, "Test Search");

        var result = await saveSearchForm.GetSearchAsync(payload);

        Assert.AreEqual(expected, result.SearchString);
        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == expected)), Times.Once);
    }

    [TestMethod]
    public async Task GetSearchAsync_WithMultipleLanguagesMilestonesDates_ParsesAndReturnsCorrectSearchString()
    {
        var url = "https://github.com/search?q=repo:microsoft/PowerToys+language:csharp+language:javascript+milestone:v1.0+milestone:v2.0+created:>2022-01-01+updated:<2023-01-01";
        var expected = "repo:microsoft/PowerToys language:csharp language:javascript milestone:v1.0 milestone:v2.0 created:>2022-01-01 updated:<2023-01-01";

        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);
        mockSearchRepository.Setup(repo => repo.UpdateSearchTopLevelStatus(It.IsAny<ISearch>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

        var mockResources = new Mock<IResources>();

        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object, savedSearchesMediator);
        var payload = CreatePayload(url, "Test Search");

        var result = await saveSearchForm.GetSearchAsync(payload);

        Assert.AreEqual(expected, result.SearchString);
        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == expected)), Times.Once);
    }

    private string CreatePayload(string input, string name)
    {
        return JsonSerializer.Serialize(new
        {
            EnteredSearch = input,
            Name = name,
            IsTopLevel = "false",
        });
    }
}
