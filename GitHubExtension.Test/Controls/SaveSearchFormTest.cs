// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Nodes;
using GitHubExtension.Controls;
using GitHubExtension.Controls.Forms;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Moq;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class SaveSearchFormTest
{
    [TestMethod]
    public void SubmitForm_ShouldRetainIsTopLevel_WhenSearchIsSaved()
    {
        // Arrange
        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository
            .Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);
        mockSearchRepository
            .Setup(repo => repo.AddSavedSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);
        mockSearchRepository
            .Setup(repo => repo.RemoveSavedSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        var savedSearch = new SearchCandidate("test search", "test name", true);
        var saveSearchForm = new SaveSearchForm(savedSearch, mockSearchRepository.Object);

        var jsonPayload = JsonNode.Parse(@"
            {
                ""EnteredSearch"": ""test search"",
                ""Name"": ""test name"",
                ""IsTopLevel"": ""true""
            }")?.ToString();

        // Act
        saveSearchForm.SubmitForm(jsonPayload, string.Empty);

        // Wait for async operations
        Thread.Sleep(100);

        // Assert
        mockSearchRepository.Verify(
            repo =>
            repo.AddSavedSearch(It.Is<SearchCandidate>(s => s.IsTopLevel == true)),
            Times.Once);
    }

    [TestMethod]
    public void SubmitForm_ShouldSaveIssueSearch_WhenIssueSearchIsProvided()
    {
        // Arrange
        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository
            .Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        ISearch? capturedSearch = null;
        mockSearchRepository
            .Setup(repo => repo.AddSavedSearch(It.IsAny<SearchCandidate>()))
            .Callback<ISearch>(s => capturedSearch = s)
            .Returns(Task.CompletedTask);

        mockSearchRepository
            .Setup(repo => repo.RemoveSavedSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object);

        var jsonPayload = JsonNode.Parse(@"
            {
                ""EnteredSearch"": ""is:issue author:username"",
                ""Name"": ""My Issue Search"",
                ""IsTopLevel"": ""false""
            }")?.ToString();

        // Act
        saveSearchForm.SubmitForm(jsonPayload, string.Empty);

        // Wait for async operations
        Thread.Sleep(100);

        // Assert
        Assert.IsNotNull(capturedSearch);
        Assert.AreEqual("is:issue author:username", capturedSearch.SearchString);
        Assert.AreEqual("My Issue Search", capturedSearch.Name);
    }

    [TestMethod]
    public void SubmitForm_ShouldSavePullRequestSearch_WhenPRSearchIsProvided()
    {
        // Arrange
        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository
            .Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        ISearch? capturedSearch = null;
        mockSearchRepository
            .Setup(repo => repo.AddSavedSearch(It.IsAny<SearchCandidate>()))
            .Callback<ISearch>(s => capturedSearch = s)
            .Returns(Task.CompletedTask);

        mockSearchRepository
            .Setup(repo => repo.RemoveSavedSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object);

        var jsonPayload = JsonNode.Parse(@"
            {
                ""EnteredSearch"": ""is:pr author:username"",
                ""Name"": ""My PR Search"",
                ""IsTopLevel"": ""false""
            }")?.ToString();

        // Act
        saveSearchForm.SubmitForm(jsonPayload, string.Empty);

        // Wait for async operations
        Thread.Sleep(100);

        // Assert
        Assert.IsNotNull(capturedSearch);
        Assert.AreEqual("is:pr author:username", capturedSearch.SearchString);
        Assert.AreEqual("My PR Search", capturedSearch.Name);
    }

    [TestMethod]
    public void SubmitForm_ShouldSaveCombinedSearch_WhenNoTypeIsProvided()
    {
        // Arrange
        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository
            .Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        ISearch? capturedSearch = null;
        mockSearchRepository
            .Setup(repo => repo.AddSavedSearch(It.IsAny<SearchCandidate>()))
            .Callback<ISearch>(s => capturedSearch = s)
            .Returns(Task.CompletedTask);

        mockSearchRepository
            .Setup(repo => repo.RemoveSavedSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object);

        var jsonPayload = JsonNode.Parse(@"
            {
                ""EnteredSearch"": ""author:username"",
                ""Name"": ""My Combined Search"",
                ""IsTopLevel"": ""false""
            }")?.ToString();

        // Act
        saveSearchForm.SubmitForm(jsonPayload, string.Empty);

        // Wait for async operations
        Thread.Sleep(100);

        // Assert
        Assert.IsNotNull(capturedSearch);
        Assert.AreEqual("author:username", capturedSearch.SearchString);
        Assert.AreEqual("My Combined Search", capturedSearch.Name);
    }

    [TestMethod]
    public void SubmitForm_ShouldEditSearchString_WhenUpdatingExistingSearch()
    {
        // Arrange
        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository
            .Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);
        mockSearchRepository
            .Setup(repo => repo.AddSavedSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);
        mockSearchRepository
            .Setup(repo => repo.RemoveSavedSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        var existingSearch = new SearchCandidate("old search", "My Search", false);
        var saveSearchForm = new SaveSearchForm(existingSearch, mockSearchRepository.Object);

        var jsonPayload = JsonNode.Parse(@"
            {
                ""EnteredSearch"": ""new search"",
                ""Name"": ""My Search"",
                ""IsTopLevel"": ""false""
            }")?.ToString();

        // Act
        saveSearchForm.SubmitForm(jsonPayload, string.Empty);

        // Wait for async operations
        Thread.Sleep(100);

        // Assert
        mockSearchRepository.Verify(
            repo =>
            repo.AddSavedSearch(It.Is<SearchCandidate>(s =>
                s.SearchString == "new search" &&
                s.Name == "My Search")),
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
        // Arrange
        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository
            .Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);
        mockSearchRepository
            .Setup(repo => repo.AddSavedSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);
        mockSearchRepository
            .Setup(repo => repo.RemoveSavedSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        var existingSearch = new SearchCandidate("my search", "Old Name", false);
        var saveSearchForm = new SaveSearchForm(existingSearch, mockSearchRepository.Object);

        var jsonPayload = JsonNode.Parse(@"
            {
                ""EnteredSearch"": ""my search"",
                ""Name"": ""New Name"",
                ""IsTopLevel"": ""false""
            }")?.ToString();

        // Act
        saveSearchForm.SubmitForm(jsonPayload, string.Empty);

        // Wait for async operations
        Thread.Sleep(100);

        // Assert
        mockSearchRepository.Verify(
            repo =>
            repo.AddSavedSearch(It.Is<SearchCandidate>(s =>
                s.SearchString == "my search" &&
                s.Name == "New Name")),
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
        // Arrange
        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository
            .Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);
        mockSearchRepository
            .Setup(repo => repo.AddSavedSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);
        mockSearchRepository
            .Setup(repo => repo.RemoveSavedSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        var existingSearch = new SearchCandidate("old search", "Old Name", false);
        var saveSearchForm = new SaveSearchForm(existingSearch, mockSearchRepository.Object);

        var jsonPayload = JsonNode.Parse(@"
            {
                ""EnteredSearch"": ""new search"",
                ""Name"": ""New Name"",
                ""IsTopLevel"": ""false""
            }")?.ToString();

        // Act
        saveSearchForm.SubmitForm(jsonPayload, string.Empty);

        // Wait for async operations
        Thread.Sleep(100);

        // Assert
        mockSearchRepository.Verify(
            repo =>
            repo.AddSavedSearch(It.Is<SearchCandidate>(s =>
                s.SearchString == "new search" &&
                s.Name == "New Name")),
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
        // Arrange
        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository
            .Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);
        mockSearchRepository
            .Setup(repo => repo.AddSavedSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);
        mockSearchRepository
            .Setup(repo => repo.RemoveSavedSearch(It.IsAny<ISearch>()))
            .Returns(Task.CompletedTask);

        var existingSearch = new SearchCandidate("my search", "My Search", false);
        var saveSearchForm = new SaveSearchForm(existingSearch, mockSearchRepository.Object);

        var jsonPayload = JsonNode.Parse(@"
            {
                ""EnteredSearch"": ""my search"",
                ""Name"": ""My Search"",
                ""IsTopLevel"": ""true""
            }")?.ToString();

        // Act
        saveSearchForm.SubmitForm(jsonPayload, string.Empty);

        // Wait for async operations
        Thread.Sleep(100);

        // Assert
        mockSearchRepository.Verify(
            repo =>
            repo.AddSavedSearch(It.Is<SearchCandidate>(s =>
                s.SearchString == "my search" &&
                s.Name == "My Search" &&
                s.IsTopLevel == true)),
            Times.Once);

        mockSearchRepository.Verify(
            repo =>
            repo.RemoveSavedSearch(It.Is<ISearch>(s =>
                s.SearchString == "my search" &&
                s.Name == "My Search")),
            Times.Once);
    }
}
