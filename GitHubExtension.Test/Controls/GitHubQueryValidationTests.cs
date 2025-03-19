// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.Controls.Forms;
using GitHubExtension.Helpers;
using Moq;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class GitHubQueryValidationTests
{
    [TestMethod]
    public async Task ValidateSearch_SupportsIsOpenKeyword()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        var mockResources = new Mock<IResources>();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object);

        var searchString = "is:open";
        var search = new SearchCandidate(searchString, "Test Search");

        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);

        await saveSearchForm.GetSearchAsync(CreatePayload(searchString, "Test Search"));

        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == searchString)), Times.Once);
    }

    // is:issue isn't a valid search. On github.com, this just returns no results
    [TestMethod]
    public async Task ValidateSearch_SupportsIsIssueKeyword()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        var mockResources = new Mock<IResources>();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object);

        var searchString = "is:issue";
        var search = new SearchCandidate(searchString, "Test Search");

        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);

        await saveSearchForm.GetSearchAsync(CreatePayload(searchString, "Test Search"));

        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == searchString)), Times.Once);
    }

    // is:pr isn't a valid search. On github.com, this just returns no results.
    [TestMethod]
    public async Task ValidateSearch_SupportsIsPullRequestKeyword()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        var mockResources = new Mock<IResources>();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object);

        var searchString = "is:pr";
        var search = new SearchCandidate(searchString, "Test Search");

        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);

        await saveSearchForm.GetSearchAsync(CreatePayload(searchString, "Test Search"));

        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == searchString)), Times.Once);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsMultipleKeywords()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        var mockResources = new Mock<IResources>();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object);

        var searchString = "is:open is:issue";
        var search = new SearchCandidate(searchString, "Test Search");

        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);

        await saveSearchForm.GetSearchAsync(CreatePayload(searchString, "Test Search"));

        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == searchString)), Times.Once);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsRepoQualifier()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        var mockResources = new Mock<IResources>();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object);

        var searchString = "is:issue repo:microsoft/PowerToys";

        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);

        await saveSearchForm.GetSearchAsync(CreatePayload(searchString, "Test Search"));

        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == searchString)), Times.Once);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsAuthorQualifier()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        var mockResources = new Mock<IResources>();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object);

        var searchString = "is:pr author:octocat";

        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);

        await saveSearchForm.GetSearchAsync(CreatePayload(searchString, "Test Search"));

        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == searchString)), Times.Once);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsStateAndLabelQualifiers()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        var mockResources = new Mock<IResources>();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object);

        var searchString = "state:open label:bug";

        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);

        await saveSearchForm.GetSearchAsync(CreatePayload(searchString, "Test Search"));

        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == searchString)), Times.Once);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsInvolvesAndLanguageQualifiers()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        var mockResources = new Mock<IResources>();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object);

        var searchString = "involves:defunkt language:javascript";

        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);

        await saveSearchForm.GetSearchAsync(CreatePayload(searchString, "Test Search"));

        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == searchString)), Times.Once);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsOrgAndCreatedQualifiers()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        var mockResources = new Mock<IResources>();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object);

        var searchString = "org:github created:>2022-01-01";

        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);

        await saveSearchForm.GetSearchAsync(CreatePayload(searchString, "Test Search"));

        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == searchString)), Times.Once);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsAssigneeAndMilestoneQualifiers()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        var mockResources = new Mock<IResources>();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object);

        var searchString = "is:issue assignee:@me milestone:v1.0";

        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);

        await saveSearchForm.GetSearchAsync(CreatePayload(searchString, "Test Search"));

        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == searchString)), Times.Once);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsReviewQualifier()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        var mockResources = new Mock<IResources>();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object);

        var searchString = "is:pr review:approved";

        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);

        await saveSearchForm.GetSearchAsync(CreatePayload(searchString, "Test Search"));

        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == searchString)), Times.Once);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsInQualifier()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        var mockResources = new Mock<IResources>();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object);

        var searchString = "is:issue in:title error";

        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);

        await saveSearchForm.GetSearchAsync(CreatePayload(searchString, "Test Search"));

        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == searchString)), Times.Once);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsMergedDateQualifier()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        var mockResources = new Mock<IResources>();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object);

        var searchString = "is:pr merged:>=2023-01-01";

        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);

        await saveSearchForm.GetSearchAsync(CreatePayload(searchString, "Test Search"));

        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == searchString)), Times.Once);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsSingleLabelQualifier()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        var mockResources = new Mock<IResources>();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object);

        var searchString = "label:enhancement";

        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);

        await saveSearchForm.GetSearchAsync(CreatePayload(searchString, "Test Search"));

        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == searchString)), Times.Once);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsMultipleLabelQualifiers()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        var mockResources = new Mock<IResources>();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object);

        var searchString = "label:bug label:help-wanted label:documentation";

        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);

        await saveSearchForm.GetSearchAsync(CreatePayload(searchString, "Test Search"));

        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == searchString)), Times.Once);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsExcludingLabel()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        var mockResources = new Mock<IResources>();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object);

        var searchString = "-label:wontfix";

        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);

        await saveSearchForm.GetSearchAsync(CreatePayload(searchString, "Test Search"));

        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == searchString)), Times.Once);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsExcludingMultipleLabels()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        var mockResources = new Mock<IResources>();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object);

        var searchString = "-label:wontfix -label:duplicate -label:invalid";

        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);

        await saveSearchForm.GetSearchAsync(CreatePayload(searchString, "Test Search"));

        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == searchString)), Times.Once);
    }

    [TestMethod]
    public async Task ValidateSearch_SupportsExcludingOtherQualifiers()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        var mockResources = new Mock<IResources>();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object);

        var searchString = "is:issue -is:closed -author:bot";

        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);

        await saveSearchForm.GetSearchAsync(CreatePayload(searchString, "Test Search"));

        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == searchString)), Times.Once);
    }

    // this a valid search that returns no results
    [TestMethod]
    public async Task ValidateSearch_SupportsMixOfIncludeAndExcludeQualifiers()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        var mockResources = new Mock<IResources>();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object);

        var searchString = "is:pr label:enhancement -label:wontfix repo:microsoft/PowerToys -is:draft";

        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);

        await saveSearchForm.GetSearchAsync(CreatePayload(searchString, "Test Search"));

        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == searchString)), Times.Once);
    }

    // this is a valid search, but returned no results
    [TestMethod]
    public async Task ValidateSearch_SupportsBooleanOperators()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        var mockResources = new Mock<IResources>();
        var saveSearchForm = new SaveSearchForm(mockSearchRepository.Object, mockResources.Object);

        var searchString = "is:open AND (is:issue OR is:pr) NOT author:bot devhome";
        var search = new SearchCandidate(searchString, "Test Search");

        mockSearchRepository.Setup(repo => repo.ValidateSearch(It.IsAny<ISearch>())).Returns(Task.CompletedTask);

        await saveSearchForm.GetSearchAsync(CreatePayload(searchString, "Test Search"));

        mockSearchRepository.Verify(repo => repo.ValidateSearch(It.Is<ISearch>(s => s.SearchString == searchString)), Times.Once);
    }

    private string CreatePayload(string searchString, string name)
    {
        return $"{{ \"EnteredSearch\": \"{searchString}\", \"Name\": \"{name}\", \"IsTopLevel\": \"false\" }}";
    }
}
