// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.DataManager;
using GitHubExtension.DataManager.Cache;
using GitHubExtension.DataManager.Data;
using GitHubExtension.DataModel.DataObjects;
using Moq;

namespace GitHubExtension.Test.DataStoreTests;

[TestClass]
public class CacheDataManagerFacadeTests
{
    [TestMethod]
    [TestCategory("Unit")]
    public void Create()
    {
        var stubCacheManager = new Mock<ICacheManager>().Object;
        var stubGitHubDataManager = new Mock<IDataRequester>().Object;
        var stubDecoratorFactory = new Mock<IDecoratorFactory>().Object;
        var cacheDataManagerFacade = new CacheDataManagerFacade(stubCacheManager, stubGitHubDataManager, stubDecoratorFactory);
        Assert.IsNotNull(cacheDataManagerFacade);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task GetIssues()
    {
        var stubCacheManager = new Mock<ICacheManager>().Object;
        var mockGitHubDataManager = new Mock<IDataRequester>();
        var stubDecoratorFactory = new Mock<IDecoratorFactory>().Object;
        var cacheDataManagerFacade = new CacheDataManagerFacade(stubCacheManager, mockGitHubDataManager.Object, stubDecoratorFactory);
        Assert.IsNotNull(cacheDataManagerFacade);

        var search = new Mock<ISearch>().Object;
        var mockIssues = new Mock<IEnumerable<Issue>>();

        mockGitHubDataManager.Setup(x => x.GetIssuesForSearch(It.IsAny<string>(), It.IsAny<string>())).Returns(mockIssues.Object);

        var issues = await cacheDataManagerFacade.GetIssues(search);
        Assert.IsNotNull(issues);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task GetPullRequests()
    {
        var stubCacheManager = new Mock<ICacheManager>().Object;
        var mockGitHubDataManager = new Mock<IDataRequester>();
        var mockDecoratorFactory = new Mock<IDecoratorFactory>();
        var cacheDataManagerFacade = new CacheDataManagerFacade(stubCacheManager, mockGitHubDataManager.Object, mockDecoratorFactory.Object);
        Assert.IsNotNull(cacheDataManagerFacade);

        mockDecoratorFactory.Setup(x => x.DecorateSearchBranch(It.IsAny<IPullRequest>()))
                            .Returns((IPullRequest pr) => pr);

        var search = new Mock<ISearch>().Object;
        var mockPullRequests = new Mock<IEnumerable<PullRequest>>();

        mockGitHubDataManager.Setup(x => x.GetPullRequestsForSearch(It.IsAny<string>(), It.IsAny<string>())).Returns(mockPullRequests.Object);

        var pullRequests = await cacheDataManagerFacade.GetPullRequests(search);
        Assert.IsNotNull(pullRequests);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task GetIssuesAndPullRequests()
    {
        var stubCacheManager = new Mock<ICacheManager>().Object;
        var mockGitHubDataManager = new Mock<IDataRequester>();
        var mockDecoratorFactory = new Mock<IDecoratorFactory>();
        var cacheDataManagerFacade = new CacheDataManagerFacade(stubCacheManager, mockGitHubDataManager.Object, mockDecoratorFactory.Object);
        Assert.IsNotNull(cacheDataManagerFacade);

        mockDecoratorFactory.Setup(x => x.DecorateSearchBranch(It.IsAny<IPullRequest>()))
                            .Returns((IPullRequest pr) => pr);

        Assert.IsNotNull(cacheDataManagerFacade);

        var search = new Mock<ISearch>().Object;

        var mockIssue1 = new Issue() { Title = "Issue1", TimeUpdated = 1 };
        var mockIssue2 = new Issue() { Title = "Issue2", TimeUpdated = 4 };
        var mockIssue3 = new Issue() { Title = "Issue3", TimeUpdated = 5 };

        var mockPullRequest1 = new PullRequest() { Title = "PullRequest1", TimeUpdated = 2 };
        var mockPullRequest2 = new PullRequest() { Title = "PullRequest2", TimeUpdated = 3 };
        var mockPullRequest3 = new PullRequest() { Title = "PullRequest3", TimeUpdated = 6 };

        var mockIssues = new List<Issue> { mockIssue1, mockIssue2, mockIssue3 };
        var mockPullRequests = new List<PullRequest> { mockPullRequest1, mockPullRequest2, mockPullRequest3 };

        mockGitHubDataManager.Setup(x => x.GetIssuesForSearch(It.IsAny<string>(), It.IsAny<string>())).Returns(mockIssues);
        mockGitHubDataManager.Setup(x => x.GetPullRequestsForSearch(It.IsAny<string>(), It.IsAny<string>())).Returns(mockPullRequests);
        var issuesAndPullRequests = await cacheDataManagerFacade.GetIssuesAndPullRequests(search);

        Assert.AreEqual(6, issuesAndPullRequests.Count());
        Assert.AreEqual("Issue1", issuesAndPullRequests.ElementAt(0).Title);
        Assert.AreEqual("PullRequest1", issuesAndPullRequests.ElementAt(1).Title);
        Assert.AreEqual("PullRequest2", issuesAndPullRequests.ElementAt(2).Title);
        Assert.AreEqual("Issue2", issuesAndPullRequests.ElementAt(3).Title);
        Assert.AreEqual("Issue3", issuesAndPullRequests.ElementAt(4).Title);
        Assert.AreEqual("PullRequest3", issuesAndPullRequests.ElementAt(5).Title);
    }
}
