// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.DataManager.Cache;
using GitHubExtension.DataManager.Data;
using GitHubExtension.DataManager.Enums;
using GitHubExtension.DataModel.Enums;
using Moq;
using Octokit;

namespace GitHubExtension.Test.DataStoreTests;

[TestClass]
public partial class CacheManagerTests
{
    [TestMethod]
    [TestCategory("Unit")]
    public void CacheManagerCreate()
    {
        var stubGitHubClient = new Mock<IGitHubCacheDataManager>().Object;
        var stubSearchRepository = new Mock<ISearchRepository>().Object;
        using var cacheManager = new CacheManager(stubGitHubClient, stubSearchRepository);
        Assert.IsNotNull(cacheManager);
    }

    private Mock<ISearchRepository> MockSearchRepository()
    {
        var mockSearchRepository = new Mock<ISearchRepository>();
        mockSearchRepository.Setup(x => x.GetSavedSearches()).ReturnsAsync(new List<ISearch>());
        return mockSearchRepository;
    }

    private Mock<IGitHubCacheDataManager> MockGitHubDataManager()
    {
        var mockGitHubDataManager = new Mock<IGitHubCacheDataManager>();
        mockGitHubDataManager.Setup(x => x.RequestAllUpdateAsync(It.IsAny<List<ISearch>>(), It.IsAny<RequestOptions>())).Returns(Task.CompletedTask);
        return mockGitHubDataManager;
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task AllUpdateStatesTest()
    {
        var mockSearchRepository = MockSearchRepository();
        var mockGitHubDataManager = MockGitHubDataManager();
        using var cacheManager = new CacheManager(mockGitHubDataManager.Object, mockSearchRepository.Object);

        Assert.AreEqual(cacheManager.IdleState, cacheManager.State);
        await cacheManager.Refresh(UpdateType.All);

        Assert.AreEqual(cacheManager.RefreshingState, cacheManager.State);
        mockGitHubDataManager.Raise(x => x.OnUpdate += null, this, new DataManagerUpdateEventArgs(DataManagerUpdateKind.Success, UpdateType.All, string.Empty, Array.Empty<string>()));

        Assert.AreEqual(cacheManager.IdleState, cacheManager.State);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task RefreshDuringUpdateTest()
    {
        var mockSearchRepository = MockSearchRepository();
        var mockGitHubDataManager = MockGitHubDataManager();
        using var cacheManager = new CacheManager(mockGitHubDataManager.Object, mockSearchRepository.Object);

        Assert.AreEqual(cacheManager.IdleState, cacheManager.State);
        await cacheManager.PeriodicUpdate();
        Assert.AreEqual(cacheManager.PeriodicUpdatingState, cacheManager.State);

        var stubSearch = new Mock<ISearch>();
        stubSearch.SetupAllProperties();

        await cacheManager.Refresh(UpdateType.Search, stubSearch.Object);

        Assert.AreEqual(cacheManager.PendingRefreshState, cacheManager.State);
        Assert.AreEqual(stubSearch.Object, cacheManager.PendingSearch);

        mockGitHubDataManager.Raise(x => x.OnUpdate += null, this, new DataManagerUpdateEventArgs(DataManagerUpdateKind.Cancel, UpdateType.All, string.Empty, Array.Empty<string>()));

        Assert.AreEqual(cacheManager.RefreshingState, cacheManager.State);

        mockGitHubDataManager.Raise(x => x.OnUpdate += null, this, new DataManagerUpdateEventArgs(DataManagerUpdateKind.Success, UpdateType.Search, string.Empty, Array.Empty<string>()));

        Assert.AreEqual(cacheManager.IdleState, cacheManager.State);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task RefreshDuringRefreshTest()
    {
        var mockSearchRepository = MockSearchRepository();
        var mockGitHubDataManager = MockGitHubDataManager();
        using var cacheManager = new CacheManager(mockGitHubDataManager.Object, mockSearchRepository.Object);

        Assert.AreEqual(cacheManager.IdleState, cacheManager.State);

        var stubSearch = new Mock<ISearch>();
        stubSearch.SetupAllProperties();

        await cacheManager.Refresh(UpdateType.Search, stubSearch.Object);

        Assert.AreEqual(cacheManager.RefreshingState, cacheManager.State);
        Assert.AreEqual(stubSearch.Object, cacheManager.PendingSearch);

        await cacheManager.Refresh(UpdateType.Search, stubSearch.Object);

        // As we passed the same search object, the refresh should be ignored.
        Assert.AreEqual(cacheManager.RefreshingState, cacheManager.State);

        mockGitHubDataManager.Raise(x => x.OnUpdate += null, this, new DataManagerUpdateEventArgs(DataManagerUpdateKind.Success, UpdateType.Search, string.Empty, Array.Empty<string>()));

        Assert.AreEqual(cacheManager.IdleState, cacheManager.State);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task RefreshDuringRefreshWithDifferentSearchTest()
    {
        var mockSearchRepository = MockSearchRepository();
        var mockGitHubDataManager = MockGitHubDataManager();
        using var cacheManager = new CacheManager(mockGitHubDataManager.Object, mockSearchRepository.Object);

        Assert.AreEqual(cacheManager.IdleState, cacheManager.State);

        var stubSearch1 = new Mock<ISearch>();
        stubSearch1.SetupGet(x => x.SearchString).Returns("Test1");

        var stubSearch2 = new Mock<ISearch>();
        stubSearch2.SetupGet(x => x.SearchString).Returns("Test2");

        await cacheManager.Refresh(UpdateType.Search, stubSearch1.Object);

        Assert.AreEqual(stubSearch1.Object, cacheManager.PendingSearch);
        Assert.AreEqual(cacheManager.RefreshingState, cacheManager.State);
        mockGitHubDataManager.Verify(x => x.RequestSearchUpdateAsync(It.IsAny<string>(), "Test1", It.IsAny<SearchType>(), It.IsAny<RequestOptions>()), Times.Once);

        await cacheManager.Refresh(UpdateType.Search, stubSearch2.Object);

        // Now as we passed a different search object, the refresh should be queued.
        Assert.AreEqual(stubSearch2.Object, cacheManager.PendingSearch);
        Assert.AreEqual(cacheManager.PendingRefreshState, cacheManager.State);

        mockGitHubDataManager.Raise(x => x.OnUpdate += null, this, new DataManagerUpdateEventArgs(DataManagerUpdateKind.Cancel, UpdateType.Search, string.Empty, Array.Empty<string>()));

        Assert.AreEqual(cacheManager.RefreshingState, cacheManager.State);
        mockGitHubDataManager.Verify(x => x.RequestSearchUpdateAsync(It.IsAny<string>(), "Test2", It.IsAny<SearchType>(), It.IsAny<RequestOptions>()), Times.Once);

        mockGitHubDataManager.Raise(x => x.OnUpdate += null, this, new DataManagerUpdateEventArgs(DataManagerUpdateKind.Success, UpdateType.Search, string.Empty, Array.Empty<string>()));

        Assert.AreEqual(cacheManager.IdleState, cacheManager.State);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task RefreshCancellationTest()
    {
        var mockSearchRepository = MockSearchRepository();
        var mockGitHubDataManager = MockGitHubDataManager();
        using var cacheManager = new CacheManager(mockGitHubDataManager.Object, mockSearchRepository.Object);

        Assert.AreEqual(cacheManager.IdleState, cacheManager.State);

        var stubSearch = new Mock<ISearch>();
        stubSearch.SetupAllProperties();
        await cacheManager.Refresh(UpdateType.Search, stubSearch.Object);

        Assert.AreEqual(cacheManager.RefreshingState, cacheManager.State);

        mockGitHubDataManager.Raise(x => x.OnUpdate += null, this, new DataManagerUpdateEventArgs(DataManagerUpdateKind.Cancel, UpdateType.All, string.Empty, Array.Empty<string>()));

        Assert.AreEqual(cacheManager.IdleState, cacheManager.State);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task PeriodicUpdateCancellationTest()
    {
        var mockSearchRepository = MockSearchRepository();
        var mockGitHubDataManager = MockGitHubDataManager();
        using var cacheManager = new CacheManager(mockGitHubDataManager.Object, mockSearchRepository.Object);

        Assert.AreEqual(cacheManager.IdleState, cacheManager.State);
        await cacheManager.PeriodicUpdate();

        Assert.AreEqual(cacheManager.PeriodicUpdatingState, cacheManager.State);

        mockGitHubDataManager.Raise(x => x.OnUpdate += null, this, new DataManagerUpdateEventArgs(DataManagerUpdateKind.Cancel, UpdateType.All, string.Empty, Array.Empty<string>()));

        Assert.AreEqual(cacheManager.IdleState, cacheManager.State);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void RequestRefreshTest()
    {
        var mockSearchRepository = MockSearchRepository();
        var mockGitHubDataManager = MockGitHubDataManager();
        using var cacheManager = new CacheManager(mockGitHubDataManager.Object, mockSearchRepository.Object);

        mockGitHubDataManager.Setup(x => x.IsSearchNewOrStale(It.IsAny<ISearch>(), It.IsAny<TimeSpan>())).Returns(false);

        var stubSearch = new Mock<ISearch>();

        cacheManager.RequestRefresh(UpdateType.Search, stubSearch.Object);

        mockGitHubDataManager.Verify(x => x.RequestSearchUpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchType>(), It.IsAny<RequestOptions>()), Times.Never);

        mockGitHubDataManager.Setup(x => x.IsSearchNewOrStale(It.IsAny<ISearch>(), It.IsAny<TimeSpan>())).Returns(true);

        cacheManager.RequestRefresh(UpdateType.Search, stubSearch.Object);

        mockGitHubDataManager.Verify(x => x.RequestSearchUpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchType>(), It.IsAny<RequestOptions>()), Times.Once);
    }
}
