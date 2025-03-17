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
    public async Task SearchPage_RateLimitExceededException()
    {
        var stubResources = new Mock<IResources>();
        var stubSearch = new Mock<ISearch>();
        stubSearch.Setup(x => x.Name).Returns("Test Search");
        stubSearch.Setup(x => x.SearchString).Returns("test search string is:issue");
        stubSearch.Setup(x => x.Type).Returns(SearchType.Issues);

        var gitHubClient = new GitHubClient(new ProductHeaderValue("TestHeader"));

        var mockDevId = new Mock<IDeveloperId>();
        mockDevId.Setup(x => x.GitHubClient).Returns(gitHubClient);

        var mockDeveloperIdProvider = new Mock<IDeveloperIdProvider>();
        mockDeveloperIdProvider.Setup(x => x.GetLoggedInDeveloperIdsInternal()).Returns(new List<IDeveloperId> { mockDevId.Object });

        var mockGitHubClientProvider = new Mock<GitHubClientProvider>(mockDeveloperIdProvider.Object);

        var validator = new GitHubValidatorAdapter(mockDeveloperIdProvider.Object);
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var dataManager = new PersistentDataManager(validator, dataStoreOptions);

        using var gitHubDataManager = new GitHubDataManager(mockGitHubClientProvider.Object, dataStoreOptions);

        var cacheManager = new CacheManager(new GitHubCacheAdapter(gitHubDataManager), dataManager);
        var cacheDataManager = new CacheDataManagerFacade(cacheManager, gitHubDataManager, new Mock<IDecoratorFactory>().Object);

        var searchPage = new IssuesSearchPage(stubSearch.Object, cacheDataManager, stubResources.Object);

        var intialLoad = searchPage.GetItems();

        try
        {
            // simulate a rate limit exceeded excpetion by going through all the calls to exceed the rate limit
            while (gitHubClient.GetLastApiInfo().RateLimit?.Remaining > 0)
            {
                await gitHubClient.Issue.GetAllForRepository("octokit", "octokit.net");
            }
        }
        catch (RateLimitExceededException)
        {
            // this is expected, continue
        }

        var miscellaneousRateLimit = await gitHubClient.RateLimit.GetRateLimits();
        var limit = miscellaneousRateLimit.Resources.Core.Limit;
        var remaining = miscellaneousRateLimit.Resources.Core.Remaining;
        var reset = miscellaneousRateLimit.Resources.Core.Reset;
        var searchLimit = miscellaneousRateLimit.Resources.Search.Limit;
        var searchRemaining = miscellaneousRateLimit.Resources.Search.Remaining;
        var searchReset = miscellaneousRateLimit.Resources.Search.Reset;

        var items = searchPage.GetItems();

        Assert.AreEqual(1, items.Length);
        Assert.AreEqual("Rate limit exceeded", items[0].Details.Title);

        dataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }
}
