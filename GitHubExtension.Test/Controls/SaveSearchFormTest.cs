// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
    public void HandleSubmit_ShouldRetainIsTopLevel_WhenSearchIsSaved()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();

        var searchCandidateAdded = false;
        var isTopLevelValue = false;

        mockSearchRepository
            .Setup(repo => repo.AddSavedSearch(It.IsAny<SearchCandidate>()))
            .Callback<SearchCandidate>(s =>
            {
                searchCandidateAdded = true;
                isTopLevelValue = s.IsTopLevel;
            })
            .Returns(Task.CompletedTask);

        mockSearchRepository
            .Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        var savedSearch = new SearchCandidate("test search", "test name", true);
        var saveSearchForm = new SaveSearchForm(savedSearch, mockSearchRepository.Object);

        var jsonPayload = JsonNode.Parse(@"
            {
                ""EnteredSearch"": ""test search"",
                ""Name"": ""test name"",
                ""IsTopLevel"": ""true""
            }")?.ToString();

        saveSearchForm.SubmitForm(jsonPayload, string.Empty);

        Thread.Sleep(100);

        mockSearchRepository.Verify(repo => repo.AddSavedSearch(It.Is<SearchCandidate>(s => s.IsTopLevel == true)), Times.Once);
        Assert.IsTrue(searchCandidateAdded, "Search candidate should be added");
    }

    [TestMethod]
    public void HandleSubmit_ShouldInvokeEvents_WhenSearchIsSaved()
    {
        // Arrange
        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository
            .Setup(repo => repo.AddSavedSearch(It.IsAny<SearchCandidate>()))
            .Returns(Task.CompletedTask);

        mockSearchRepository
            .Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        var savedSearch = new SearchCandidate("test search", "test name", true);
        var saveSearchForm = new SaveSearchForm(savedSearch, mockSearchRepository.Object);

        var loadingChangedInvoked = false;
        var formSubmittedInvoked = false;
        var searchSavedInvoked = false;

        saveSearchForm.LoadingStateChanged += (sender, isLoading) =>
        {
            if (!isLoading)
            {
                loadingChangedInvoked = true;
            }
        };

        saveSearchForm.FormSubmitted += (sender, args) =>
        {
            if (args.Status)
            {
                formSubmittedInvoked = true;
            }
        };

        SaveSearchForm.SearchSaved += (sender, search) =>
        {
            if (search is SearchCandidate)
            {
                searchSavedInvoked = true;
            }
        };

        var jsonPayload = JsonNode.Parse(@"
            {
                ""EnteredSearch"": ""test search"",
                ""Name"": ""test name"",
                ""IsTopLevel"": ""true""
            }")?.ToString();

        // Act
        saveSearchForm.SubmitForm(jsonPayload, string.Empty);

        // Give time for the async operation to complete
        Thread.Sleep(100);

        // Assert
        Assert.IsTrue(loadingChangedInvoked, "LoadingStateChanged event should be invoked");
        Assert.IsTrue(formSubmittedInvoked, "FormSubmitted event should be invoked");
        Assert.IsTrue(searchSavedInvoked, "SearchSaved event should be invoked");

        // Clean up event handler
        SaveSearchForm.SearchSaved -= (sender, search) => { };
    }
}
