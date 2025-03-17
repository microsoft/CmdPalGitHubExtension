// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Net;
using GitHubExtension.Client;
using GitHubExtension.Controls;
using GitHubExtension.Controls.Pages;
using GitHubExtension.DataManager;
using GitHubExtension.DataManager.Cache;
using GitHubExtension.DataManager.Data;
using GitHubExtension.DataModel;
using GitHubExtension.DataModel.DataObjects;
using GitHubExtension.DataModel.Enums;
using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using GitHubExtension.PersistentData;
using GitHubExtension.Test.Helpers;
using GitHubExtension.Test.PersistentData;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Moq;
using Octokit;
using Octokit.Internal;
using Windows.Media.Protection.PlayReady;

namespace GitHubExtension.Test.Controls;

[TestClass]
public partial class SearchPagesTests
{
    [TestMethod]
    [TestCategory("Unit")]
    public void SearchPagesCreate()
    {
        var stubCacheDataManager = new Mock<ICacheDataManager>().Object;
        var stubResources = new Mock<IResources>().Object;
        var stubSearch = new Mock<ISearch>();
        stubSearch.Setup(x => x.Name).Returns("Name");
        stubSearch.Setup(x => x.SearchString).Returns("test search string is:pr");
        stubSearch.Setup(x => x.Type).Returns(SearchType.Issues);
        var issuesSearchPage = new IssuesSearchPage(stubSearch.Object, stubCacheDataManager, stubResources);
        Assert.IsNotNull(issuesSearchPage);

        stubSearch.Setup(x => x.Type).Returns(SearchType.PullRequests);
        var pullRequestsSearchPage = new PullRequestsSearchPage(stubSearch.Object, stubCacheDataManager, stubResources);
        Assert.IsNotNull(pullRequestsSearchPage);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GetItemsFromPullRequestPage()
    {
        var stubCacheDataManager = new Mock<ICacheDataManager>();
        var stubResources = new Mock<IResources>().Object;
        var stubSearch = new Mock<ISearch>();
        stubSearch.Setup(x => x.Name).Returns("Name");
        stubSearch.Setup(x => x.SearchString).Returns("test search string is:pr");
        stubSearch.Setup(x => x.Type).Returns(SearchType.PullRequests);

        var pullRequestsSearchPage = new PullRequestsSearchPage(stubSearch.Object, stubCacheDataManager.Object, stubResources);

        var pull1 = new Mock<IPullRequest>();
        var pull2 = new Mock<IPullRequest>();
        pull1.Setup(x => x.Title).Returns("Title1");
        pull1.Setup(x => x.HtmlUrl).Returns("mock/url1");
        pull1.Setup(x => x.Number).Returns(1);
        pull2.Setup(x => x.Title).Returns("Title2");
        pull2.Setup(x => x.HtmlUrl).Returns("mock/url2");
        pull2.Setup(x => x.Number).Returns(2);

        var pullRequests = new List<IPullRequest>
        {
            pull1.Object,
            pull2.Object,
        };

        stubCacheDataManager.Setup(x => x.GetPullRequests(stubSearch.Object)).ReturnsAsync(pullRequests);

        var items = pullRequestsSearchPage.GetItems();

        Assert.AreEqual(pullRequests.Count, items.Length);
        Assert.AreEqual(pullRequests[0].Title, items[0].Title);
        Assert.AreEqual(pullRequests[1].Title, items[1].Title);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GetItemsFromIssuesPage()
    {
        var stubCacheDataManager = new Mock<ICacheDataManager>();
        var stubResources = new Mock<IResources>().Object;
        var stubSearch = new Mock<ISearch>();
        stubSearch.Setup(x => x.Name).Returns("Name");
        stubSearch.Setup(x => x.SearchString).Returns("test search string is:issue");
        stubSearch.Setup(x => x.Type).Returns(SearchType.Issues);

        var issuesSearchPage = new IssuesSearchPage(stubSearch.Object, stubCacheDataManager.Object, stubResources);

        var issue1 = new Mock<IIssue>();
        var issue2 = new Mock<IIssue>();
        issue1.Setup(x => x.Title).Returns("Title1");
        issue1.Setup(x => x.HtmlUrl).Returns("mock/url1");
        issue1.Setup(x => x.Number).Returns(1);
        issue2.Setup(x => x.Title).Returns("Title2");
        issue2.Setup(x => x.HtmlUrl).Returns("mock/url2");
        issue2.Setup(x => x.Number).Returns(2);

        var issues = new List<IIssue>
        {
            issue1.Object,
            issue2.Object,
        };

        stubCacheDataManager.Setup(x => x.GetIssues(stubSearch.Object)).ReturnsAsync(issues);

        var items = issuesSearchPage.GetItems();

        Assert.AreEqual(issues.Count, items.Length);
        Assert.AreEqual(issues[0].Title, items[0].Title);
        Assert.AreEqual(issues[1].Title, items[1].Title);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task GetClientForLoggedInDeveloper_RateLimitExceededException()
    {
        var stubDeveloperIdProvider = new Mock<IDeveloperIdProvider>();
        var stubDeveloperId = new Mock<IDeveloperId>();
        var stubGitHubClient = new Mock<GitHubClient>(new ProductHeaderValue("TestApp"), new InMemoryCredentialStore(Credentials.Anonymous));

        stubDeveloperId.Setup(x => x.GitHubClient).Returns(stubGitHubClient.Object);
        stubDeveloperIdProvider.Setup(x => x.GetLoggedInDeveloperIdsInternal()).Returns(new List<IDeveloperId> { stubDeveloperId.Object });

        var mockResponse = new Mock<IResponse>();
        mockResponse.SetupGet(r => r.StatusCode).Returns(HttpStatusCode.Forbidden);
        mockResponse.SetupGet(r => r.Body).Returns(string.Empty);
        mockResponse.SetupGet(r => r.Headers).Returns(new Dictionary<string, string>());

        var mockRateLimit = new Mock<RateLimit>(100, 0, DateTimeOffset.Now.AddHours(1));
        var mockApiInfo = new Mock<ApiInfo>(
            "etag",
            mockRateLimit.Object,
            new List<string> { "scope1", "scope2" },
            new List<string> { "acceptedScope1", "acceptedScope2" },
            new Dictionary<string, Uri> { { "self", new Uri("https://api.github.com") } });
        mockResponse.SetupGet(r => r.ApiInfo).Returns(mockApiInfo.Object);

        var rateLimitException = new RateLimitExceededException(mockResponse.Object);
        stubGitHubClient.Setup(x => x.RateLimit.GetRateLimits()).ThrowsAsync(rateLimitException);

        var gitHubClientProvider = new GitHubClientProvider(stubDeveloperIdProvider.Object);

        var client = await gitHubClientProvider.GetClientForLoggedInDeveloper(logRateLimit: true);

        Assert.IsNotNull(client);
        Assert.AreEqual(stubGitHubClient.Object, client);
        stubGitHubClient.Verify(x => x.RateLimit.GetRateLimits(), Times.Once);
    }
}
