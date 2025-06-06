// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Nodes;
using GitHubExtension.Controls;
using GitHubExtension.Controls.Forms;
using GitHubExtension.DataModel.Enums;
using GitHubExtension.Helpers;
using GitHubExtension.Test.Helpers;
using Moq;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class SaveSearchFormTest
{
    private (Mock<ISearchRepository> MockSearchRepository, IResources Resources, SavedSearchesMediator Mediator) CreateSaveSearchFormTestMocks()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        var mockResources = new Mock<IResources>().Object;
        var mediator = new SavedSearchesMediator();
        return (mockSearchRepository, mockResources, mediator);
    }

    // This tests the CreateSearchFromJson() method in SaveSearchForm, not the private method below
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

    [DataRow("is:issue author:testuser", "ShouldSaveIssueSearch_WhenIssueSearchIsProvided", true)]
    [DataRow("is:pr author:testuser", "ShouldSavePullRequestSearch_WhenPRSearchIsProvided", false)]
    [DataRow("author:testuser", "ShouldSaveCombinedSearch_WhenCombinedSearchIsProvided", false)]
    [TestMethod]
    public async Task Submit_FormSavesSearchCorrectly(string enteredSearchString, string enteredSearchName, bool enteredSearchIsTopLevel)
    {
        var (mockSearchRepository, mockResources, savedSearchesMediator) = CreateSaveSearchFormTestMocks();

        ISearch? capturedSearch = null;
        bool? capturedSearchIsTopLevel = null;
        SearchType? capturedSearchType = null;
        mockSearchRepository
            .Setup(repo => repo.UpdateSearchTopLevelStatus(It.IsAny<SearchCandidate>(), It.IsAny<bool>()))
            .Callback<ISearch, bool>((s, isTopLevel) =>
            {
                capturedSearch = s;
                capturedSearchIsTopLevel = isTopLevel;
                capturedSearchType = s.Type;
            })
            .Returns(Task.CompletedTask);

        mockSearchRepository
            .Setup(repo => repo.RemoveSavedSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        var expectedSearchType = TestHelpers.GetExpectedSearchType(enteredSearchString);
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources, savedSearchesMediator);
        var jsonPayload = TestHelpers.CreateJsonPayload(enteredSearchString, enteredSearchName, enteredSearchIsTopLevel);

        var tcs = TestHelpers.CreateTaskCompletionSource(savedSearchesMediator);
        saveSearchForm.SubmitForm(jsonPayload, string.Empty);
        await tcs.Task;

        Assert.IsNotNull(capturedSearch);
        Assert.AreEqual(enteredSearchString, capturedSearch.SearchString);
        Assert.AreEqual(enteredSearchName, capturedSearch.Name);
        Assert.AreEqual(enteredSearchIsTopLevel, capturedSearchIsTopLevel);
        Assert.AreEqual(expectedSearchType, capturedSearchType);
    }

    [TestMethod]
    [DataRow("old search", "My Search", false, "new search", "my search", false)]
    [DataRow("my search", "My Search", false, "my search", "My Search", true)]
    public async Task SubmitForm_EditsCorrectly(string previousSearchString, string previousSearchName, bool previousSearchIsTopLevel, string newSearchString, string newSearchName, bool newSearchIsTopLevel)
    {
        var (mockSearchRepository, mockResources, savedSearchesMediator) = CreateSaveSearchFormTestMocks();
        var capturedSearch = default(ISearch);
        bool? capturedSearchIsTopLevel = null;
        SearchType? capturedSearchType = default;
        var expectedSearchType = TestHelpers.GetExpectedSearchType(newSearchString);

        mockSearchRepository
            .Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);
        mockSearchRepository
            .Setup(repo => repo.UpdateSearchTopLevelStatus(It.IsAny<SearchCandidate>(), It.IsAny<bool>()))
            .Callback<ISearch, bool>((s, isTopLevel) =>
            {
                capturedSearch = s;
                capturedSearchIsTopLevel = isTopLevel;
                capturedSearchType = s.Type;
            })
            .Returns(Task.CompletedTask);
        mockSearchRepository
            .Setup(repo => repo.RemoveSavedSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        var existingSearch = new SearchCandidate(previousSearchString, previousSearchName, previousSearchIsTopLevel);
        var saveSearchForm = new SaveSearchForm(existingSearch, mockSearchRepository.Object, mockResources, savedSearchesMediator);

        var jsonPayload = TestHelpers.CreateJsonPayload(newSearchString, newSearchName, newSearchIsTopLevel);

        var tcs = TestHelpers.CreateTaskCompletionSource(savedSearchesMediator);
        saveSearchForm.SubmitForm(jsonPayload, string.Empty);
        await tcs.Task;

        mockSearchRepository.Verify(
            repo =>
            repo.UpdateSearchTopLevelStatus(
                It.Is<SearchCandidate>(s =>
                s.SearchString == newSearchString &&
                s.Name == newSearchName),
                newSearchIsTopLevel),
            Times.Once);

        mockSearchRepository.Verify(
            repo =>
            repo.RemoveSavedSearch(It.Is<ISearch>(s =>
                s.SearchString == previousSearchString &&
                s.Name == previousSearchName)),
            Times.Once);

        Assert.IsNotNull(capturedSearch);
        Assert.AreEqual(newSearchString, capturedSearch.SearchString);
        Assert.AreEqual(newSearchName, capturedSearch.Name);
        Assert.AreEqual(newSearchIsTopLevel, capturedSearchIsTopLevel);
        Assert.AreEqual(expectedSearchType, capturedSearchType);
    }
}
