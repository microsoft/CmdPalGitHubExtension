// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Net;
using GitHubExtension.Controls;
using GitHubExtension.Controls.Commands;
using GitHubExtension.Controls.Pages;
using GitHubExtension.DataManager;
using GitHubExtension.DataModel.Enums;
using GitHubExtension.Helpers;
using Moq;
using Octokit;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class SearchPagesTests
{
    private static (MutationCommandsFactory Factory, MutationMediator Mediator) CreateMutationDeps(Mock<IResources> resources)
    {
        var mutationManager = new Mock<IGitHubMutationManager>();
        var createManager = new Mock<IGitHubCreateManager>();
        var autoMergeManager = new Mock<IGitHubAutoMergeManager>();
        var mediator = new MutationMediator();
        var factory = new MutationCommandsFactory(mutationManager.Object, createManager.Object, autoMergeManager.Object, mediator, resources.Object);
        return (factory, mediator);
    }

    private (Mock<ICacheDataManager> CacheDataManager, Mock<IResources> Resources, Mock<ISearch> Search) CreateCommonMocks(SearchType type, string searchString = "test search string")
    {
        var cacheDataManager = new Mock<ICacheDataManager>();
        var resources = new Mock<IResources>();
        var search = new Mock<ISearch>();
        search.Setup(x => x.Name).Returns("Name");
        search.Setup(x => x.SearchString).Returns(searchString);
        search.Setup(x => x.Type).Returns(type);
        resources.Setup(x => x.GetResource(It.IsAny<string>(), null)).Returns("Mocked Resource");
        resources.Setup(x => x.GetResource("Commands_Copy_GitCheckoutCommand", null)).Returns("git checkout {0}");
        return (cacheDataManager, resources, search);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void SearchPagesCreate_CreatesPagesForBothTypes()
    {
        var (cacheDataManager, resources, search) = CreateCommonMocks(SearchType.Issues, "test search string is:pr");
        var (mutationFactory, mutationMediator) = CreateMutationDeps(resources);
        var issuesSearchPage = new IssuesSearchPage(search.Object, cacheDataManager.Object, resources.Object, mutationFactory, mutationMediator);
        Assert.IsNotNull(issuesSearchPage);

        search.Setup(x => x.Type).Returns(SearchType.PullRequests);
        var pullRequestsSearchPage = new PullRequestsSearchPage(search.Object, cacheDataManager.Object, resources.Object, mutationFactory, mutationMediator);
        Assert.IsNotNull(pullRequestsSearchPage);

        search.Setup(x => x.Type).Returns(SearchType.Repositories);
        var repositoriesSearchPage = new RepositoriesSearchPage(search.Object, cacheDataManager.Object, resources.Object);
        Assert.IsNotNull(repositoriesSearchPage);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GetItemsFromRepositoriesSearchPage_ReturnsExpectedItems()
    {
        var (cacheDataManager, resources, search) = CreateCommonMocks(SearchType.Repositories, "test search string type:repository");

        var page = new RepositoriesSearchPage(search.Object, cacheDataManager.Object, resources.Object);

        var repo1 = new Mock<IRepository>();
        var repo2 = new Mock<IRepository>();
        repo1.Setup(x => x.FullName).Returns("owner/repo1");
        repo1.Setup(x => x.Description).Returns("Description1");
        repo1.Setup(x => x.HtmlUrl).Returns("mock/url1");
        repo1.Setup(x => x.CloneUrl).Returns("mock/url1.git");
        repo2.Setup(x => x.FullName).Returns("owner/repo2");
        repo2.Setup(x => x.Description).Returns("Description2");
        repo2.Setup(x => x.HtmlUrl).Returns("mock/url2");
        repo2.Setup(x => x.CloneUrl).Returns("mock/url2.git");
        var repositories = new List<IRepository> { repo1.Object, repo2.Object };
        cacheDataManager.Setup(x => x.GetRepositories(search.Object)).ReturnsAsync(repositories);

        var items = page.GetItems();
        Assert.AreEqual(repositories.Count, items.Length);
        Assert.AreEqual(repositories[0].FullName, items[0].Title);
        Assert.AreEqual(repositories[1].FullName, items[1].Title);
    }

    [DataRow(SearchType.PullRequests)]
    [DataRow(SearchType.Issues)]
    [TestMethod]
    [TestCategory("Unit")]
    public void GetItemsFromSearchPage_ReturnsExpectedItems(SearchType type)
    {
        var (cacheDataManager, resources, search) = CreateCommonMocks(type, type == SearchType.PullRequests ? "test search string is:pr" : "test search string is:issue");

        if (type == SearchType.PullRequests)
        {
            var (mutationFactory, mutationMediator) = CreateMutationDeps(resources);
            var page = new PullRequestsSearchPage(search.Object, cacheDataManager.Object, resources.Object, mutationFactory, mutationMediator);
            var pull1 = new Mock<IPullRequest>();
            var pull2 = new Mock<IPullRequest>();
            pull1.Setup(x => x.Title).Returns("Title1");
            pull1.Setup(x => x.HtmlUrl).Returns("mock/url1");
            pull1.Setup(x => x.Number).Returns(1);
            pull1.Setup(pull1 => pull1.SourceBranch).Returns("source-branch1");
            pull2.Setup(x => x.Title).Returns("Title2");
            pull2.Setup(x => x.HtmlUrl).Returns("mock/url2");
            pull2.Setup(x => x.Number).Returns(2);
            pull2.Setup(pull2 => pull2.SourceBranch).Returns("source-branch2");
            var pulls = new List<IPullRequest> { pull1.Object, pull2.Object };
            cacheDataManager.Setup(x => x.GetPullRequests(search.Object)).ReturnsAsync(pulls);

            var items = page.GetItems();
            Assert.AreEqual(pulls.Count, items.Length);
            Assert.AreEqual(pulls[0].Title, items[0].Title);
            Assert.AreEqual(pulls[1].Title, items[1].Title);
        }
        else
        {
            var (mutationFactory, mutationMediator) = CreateMutationDeps(resources);
            var page = new IssuesSearchPage(search.Object, cacheDataManager.Object, resources.Object, mutationFactory, mutationMediator);
            var issue1 = new Mock<IIssue>();
            var issue2 = new Mock<IIssue>();
            issue1.Setup(x => x.Title).Returns("Title1");
            issue1.Setup(x => x.HtmlUrl).Returns("mock/url1");
            issue1.Setup(x => x.Number).Returns(1);
            issue2.Setup(x => x.Title).Returns("Title2");
            issue2.Setup(x => x.HtmlUrl).Returns("mock/url2");
            issue2.Setup(x => x.Number).Returns(2);
            var issues = new List<IIssue> { issue1.Object, issue2.Object };
            cacheDataManager.Setup(x => x.GetIssues(search.Object)).ReturnsAsync(issues);

            var items = page.GetItems();
            Assert.AreEqual(issues.Count, items.Length);
            Assert.AreEqual(issues[0].Title, items[0].Title);
            Assert.AreEqual(issues[1].Title, items[1].Title);
        }
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void SearchPageGetItems_RateLimitExceededExceptionIsCaught()
    {
        var (cacheDataManager, resources, search) = CreateCommonMocks(SearchType.PullRequests, "test search string is:pr");
        resources.Setup(x => x.GetResource("Pages_Error_Title", null)).Returns("Error fetching items");

        var (mutationFactory, mutationMediator) = CreateMutationDeps(resources);
        var pullRequestsSearchPage = new PullRequestsSearchPage(search.Object, cacheDataManager.Object, resources.Object, mutationFactory, mutationMediator);

        var mockResponse = new Mock<IResponse>();
        mockResponse.SetupGet(r => r.StatusCode).Returns(HttpStatusCode.Forbidden);
        mockResponse.SetupGet(r => r.Body).Returns(string.Empty);
        mockResponse.SetupGet(r => r.Headers).Returns(new Dictionary<string, string>());
        var mockRateLimit = new RateLimit(100, 0, DateTimeOffset.Now.AddHours(1).Ticks);
        var mockApiInfo = new ApiInfo(
            new Dictionary<string, Uri> { { "self", new Uri("https://api.github.com") } },
            new List<string> { "scope1", "scope2" },
            new List<string> { "acceptedScope1", "acceptedScope2" },
            "etag",
            mockRateLimit);
        mockResponse.SetupGet(r => r.ApiInfo).Returns(mockApiInfo);
        var rateLimitException = new RateLimitExceededException(mockResponse.Object);
        cacheDataManager.Setup(x => x.GetPullRequests(search.Object)).ThrowsAsync(rateLimitException);

        var items = pullRequestsSearchPage.GetItems();

        Assert.AreEqual(1, items.Length);
        Assert.AreEqual("Error fetching items", items[0].Title);
        Assert.AreEqual("API Rate Limit exceeded", items[0].Details.Title);
    }
}
