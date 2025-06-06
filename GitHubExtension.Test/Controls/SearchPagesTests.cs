// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Net;
using GitHubExtension.Controls;
using GitHubExtension.Controls.Pages;
using GitHubExtension.DataModel.Enums;
using GitHubExtension.Helpers;
using Moq;
using Octokit;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class SearchPagesTests
{
    private (Mock<ICacheDataManager> CacheDataManager, Mock<IResources> Resources, Mock<ISearch> Search) CreateCommonMocks(SearchType type, string searchString = "test search string")
    {
        var cacheDataManager = new Mock<ICacheDataManager>();
        var resources = new Mock<IResources>();
        var search = new Mock<ISearch>();
        search.Setup(x => x.Name).Returns("Name");
        search.Setup(x => x.SearchString).Returns(searchString);
        search.Setup(x => x.Type).Returns(type);
        return (cacheDataManager, resources, search);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void SearchPagesCreate_CreatesPagesForBothTypes()
    {
        var (cacheDataManager, resources, search) = CreateCommonMocks(SearchType.Issues, "test search string is:pr");
        var issuesSearchPage = new IssuesSearchPage(search.Object, cacheDataManager.Object, resources.Object);
        Assert.IsNotNull(issuesSearchPage);

        search.Setup(x => x.Type).Returns(SearchType.PullRequests);
        var pullRequestsSearchPage = new PullRequestsSearchPage(search.Object, cacheDataManager.Object, resources.Object);
        Assert.IsNotNull(pullRequestsSearchPage);
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
            var page = new PullRequestsSearchPage(search.Object, cacheDataManager.Object, resources.Object);
            var pull1 = new Mock<IPullRequest>();
            var pull2 = new Mock<IPullRequest>();
            pull1.Setup(x => x.Title).Returns("Title1");
            pull1.Setup(x => x.HtmlUrl).Returns("mock/url1");
            pull1.Setup(x => x.Number).Returns(1);
            pull2.Setup(x => x.Title).Returns("Title2");
            pull2.Setup(x => x.HtmlUrl).Returns("mock/url2");
            pull2.Setup(x => x.Number).Returns(2);
            var pulls = new List<IPullRequest> { pull1.Object, pull2.Object };
            cacheDataManager.Setup(x => x.GetPullRequests(search.Object)).ReturnsAsync(pulls);

            var items = page.GetItems();
            Assert.AreEqual(pulls.Count, items.Length);
            Assert.AreEqual(pulls[0].Title, items[0].Title);
            Assert.AreEqual(pulls[1].Title, items[1].Title);
        }
        else
        {
            var page = new IssuesSearchPage(search.Object, cacheDataManager.Object, resources.Object);
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

        var pullRequestsSearchPage = new PullRequestsSearchPage(search.Object, cacheDataManager.Object, resources.Object);

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
