// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Nodes;
using GitHubExtension.Controls;
using GitHubExtension.Controls.Forms;
using GitHubExtension.DataModel.Enums;
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

    [DataRow("is:issue author:testuser", "ShouldSaveIssueSearch_WhenIssueSearchIsProvided", true)]
    [DataRow("is:pr author:testuser", "ShouldSavePullRequestSearch_WhenPRSearchIsProvided", false)]
    [DataRow("author:testuser", "ShouldSaveCombinedSearch_WhenCombinedSearchIsProvided", false)]
    [TestMethod]
    public async Task Submit_FormSavesSearchCorrectly(string enteredSearchString, string enteredSearchName, bool enteredSearchIsTopLevel)
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository
            .Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

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

        var expectedSearchType = GetExpectedSearchType(enteredSearchString);

        var stubResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, stubResources, savedSearchesMediator);

        var jsonPayload = CreateJsonPayload(enteredSearchString, enteredSearchName, enteredSearchIsTopLevel);

        var tcs = CreateTaskCompletionSource(savedSearchesMediator);
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

    public async Task SubmitForm_EditsCorrectly(string previousSearchString, string previousSearchName, bool previousSearchIsTopLevel, string newSearchString, string newSearchName, bool newSearchIsTopLevel)
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository
            .Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);
        mockSearchRepository
            .Setup(repo => repo.UpdateSearchTopLevelStatus(It.IsAny<ISearch>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);
        mockSearchRepository
            .Setup(repo => repo.RemoveSavedSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        var existingSearch = new SearchCandidate(previousSearchString, previousSearchName, previousSearchIsTopLevel);
        var stubResources = new Mock<IResources>().Object;
        var savedSearchesMediator = new SavedSearchesMediator();
        var saveSearchForm = new SaveSearchForm(existingSearch, mockSearchRepository.Object, stubResources, savedSearchesMediator);

        var jsonPayload = CreateJsonPayload(newSearchString, newSearchName, newSearchIsTopLevel);

        var tcs = CreateTaskCompletionSource(savedSearchesMediator);
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
    }

    [TestMethod]
    public async Task SubmitForm_ShouldOnlyUpdateTopLevel_WhenNothingElseChanges()
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

        var tcs = CreateTaskCompletionSource(savedSearchesMediator);
        saveSearchForm.SubmitForm(jsonPayload, string.Empty);
        await tcs.Task;

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

    private SearchType GetExpectedSearchType(string enteredSearchString)
    {
        return enteredSearchString switch
        {
            var s when s.StartsWith("is:issue", StringComparison.OrdinalIgnoreCase) => SearchType.Issues,
            var s when s.StartsWith("is:pr", StringComparison.OrdinalIgnoreCase) => SearchType.PullRequests,
            _ => SearchType.IssuesAndPullRequests,
        };
    }

    private string? CreateJsonPayload(string enteredSearch, string name, bool isTopLevel)
    {
        return JsonNode.Parse($@"
        {{
            ""EnteredSearch"": ""{enteredSearch}"",
            ""Name"": ""{name}"",
            ""IsTopLevel"": ""{isTopLevel.ToString().ToLowerInvariant()}""
        }}")?.ToString();
    }

    private static TaskCompletionSource CreateTaskCompletionSource(SavedSearchesMediator savedSearchesMediator)
    {
        var tcs = new TaskCompletionSource();
        EventHandler<object?>? handler = null;
        handler = (sender, args) =>
        {
            savedSearchesMediator.SearchSaved -= handler;
            tcs.TrySetResult();
        };

        savedSearchesMediator.SearchSaved += handler;
        return tcs;
    }
}
