// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataManager;
using GitHubExtension.DataModel.Enums;
using GitHubExtension.Helpers;
using Moq;
using Octokit;

namespace GitHubExtension.Test;

public partial class DataStoreTests
{
    [TestMethod]
    [TestCategory("Unit")]
    public void CacheManagerCreate()
    {
        var stubGitHubClient = new Mock<IGitHubDataManager>().Object;
        var stubSearchHelper = new Mock<ISearchHelper>().Object;
        var stubRepositoryHelper = new Mock<IRepositoryHelper>().Object;
        using var cacheManager = new CacheManager(stubGitHubClient, stubRepositoryHelper, stubSearchHelper);
        Assert.IsNotNull(cacheManager);
    }

    private Mock<ISearchHelper> MockSearchHelper()
    {
        var mockSearchHelper = new Mock<ISearchHelper>();
        mockSearchHelper.Setup(x => x.GetSavedSearches()).ReturnsAsync(new List<PersistentData.Search>());
        return mockSearchHelper;
    }

    private Mock<IRepositoryHelper> MockRepositoryHelper()
    {
        var mockRepositoryHelper = new Mock<IRepositoryHelper>();
        mockRepositoryHelper.Setup(x => x.GetUserRepositoryCollection()).Returns(new RepositoryCollection());
        return mockRepositoryHelper;
    }

    private Mock<IGitHubDataManager> MockGitHubDataManager()
    {
        var mockGitHubDataManager = new Mock<IGitHubDataManager>();
        mockGitHubDataManager.Setup(x => x.RequestAllUpdateAsync(It.IsAny<RepositoryCollection>(), It.IsAny<List<PersistentData.Search>>(), It.IsAny<RequestOptions>())).Returns(Task.CompletedTask);
        return mockGitHubDataManager;
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task CacheManagerAllUpdateStatesTest()
    {
        var mockSearchHelper = MockSearchHelper();
        var mockRepositoryHelper = MockRepositoryHelper();
        var mockGitHubDataManager = MockGitHubDataManager();
        using var cacheManager = new CacheManager(mockGitHubDataManager.Object, mockRepositoryHelper.Object, mockSearchHelper.Object);

        Assert.AreEqual(cacheManager.IdleState, cacheManager.State);
        await cacheManager.Refresh(UpdateType.All);

        Assert.AreEqual(cacheManager.RefreshingState, cacheManager.State);
        mockGitHubDataManager.Raise(x => x.OnUpdate += null, this, new DataManagerUpdateEventArgs(DataManagerUpdateKind.Success, UpdateType.All, string.Empty, Array.Empty<string>()));

        Thread.Sleep(1000);
        Assert.AreEqual(cacheManager.IdleState, cacheManager.State);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task CacheManagerRefreshDuringUpdateTest()
    {
        var mockSearchHelper = MockSearchHelper();
        var mockRepositoryHelper = MockRepositoryHelper();
        var mockGitHubDataManager = MockGitHubDataManager();
        using var cacheManager = new CacheManager(mockGitHubDataManager.Object, mockRepositoryHelper.Object, mockSearchHelper.Object);

        Assert.AreEqual(cacheManager.IdleState, cacheManager.State);
        await cacheManager.PeriodicUpdate();
        Assert.AreEqual(cacheManager.PeriodicUpdatingState, cacheManager.State);

        var stubSearch = new Mock<PersistentData.Search>();
        stubSearch.SetupAllProperties();

        await cacheManager.Refresh(UpdateType.Search, stubSearch.Object);

        Assert.AreEqual(cacheManager.PendingRefreshState, cacheManager.State);
        Assert.AreEqual(stubSearch.Object, cacheManager.PendingSearch);

        mockGitHubDataManager.Raise(x => x.OnUpdate += null, this, new DataManagerUpdateEventArgs(DataManagerUpdateKind.Cancel, UpdateType.All, string.Empty, Array.Empty<string>()));

        Thread.Sleep(1000);
        Assert.AreEqual(cacheManager.RefreshingState, cacheManager.State);

        mockGitHubDataManager.Raise(x => x.OnUpdate += null, this, new DataManagerUpdateEventArgs(DataManagerUpdateKind.Success, UpdateType.Search, string.Empty, Array.Empty<string>()));

        Thread.Sleep(1000);
        Assert.AreEqual(cacheManager.IdleState, cacheManager.State);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task CacheManagerRefreshDuringRefreshTest()
    {
        var mockSearchHelper = MockSearchHelper();
        var mockRepositoryHelper = MockRepositoryHelper();
        var mockGitHubDataManager = MockGitHubDataManager();
        using var cacheManager = new CacheManager(mockGitHubDataManager.Object, mockRepositoryHelper.Object, mockSearchHelper.Object);

        Assert.AreEqual(cacheManager.IdleState, cacheManager.State);

        var stubSearch = new Mock<PersistentData.Search>();
        stubSearch.SetupAllProperties();

        await cacheManager.Refresh(UpdateType.Search, stubSearch.Object);

        Assert.AreEqual(cacheManager.RefreshingState, cacheManager.State);
        Assert.AreEqual(stubSearch.Object, cacheManager.PendingSearch);

        await cacheManager.Refresh(UpdateType.Search, stubSearch.Object);

        // As we passed the same search object, the refresh should be ignored.
        Assert.AreEqual(cacheManager.RefreshingState, cacheManager.State);

        mockGitHubDataManager.Raise(x => x.OnUpdate += null, this, new DataManagerUpdateEventArgs(DataManagerUpdateKind.Success, UpdateType.Search, string.Empty, Array.Empty<string>()));

        Thread.Sleep(1000);
        Assert.AreEqual(cacheManager.IdleState, cacheManager.State);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task CacheManagerRefreshDuringRefreshWithDifferentSearchTest()
    {
        var mockSearchHelper = MockSearchHelper();
        var mockRepositoryHelper = MockRepositoryHelper();
        var mockGitHubDataManager = MockGitHubDataManager();
        using var cacheManager = new CacheManager(mockGitHubDataManager.Object, mockRepositoryHelper.Object, mockSearchHelper.Object);

        Assert.AreEqual(cacheManager.IdleState, cacheManager.State);

        var stubSearch1 = new Mock<PersistentData.Search>();
        stubSearch1.SetupAllProperties();
        stubSearch1.Object.SearchString = "Test1";

        var stubSearch2 = new Mock<PersistentData.Search>();
        stubSearch2.SetupAllProperties();
        stubSearch2.Object.SearchString = "Test2";

        await cacheManager.Refresh(UpdateType.Search, stubSearch1.Object);

        Assert.AreEqual(stubSearch1.Object, cacheManager.PendingSearch);
        Assert.AreEqual(cacheManager.RefreshingState, cacheManager.State);
        mockGitHubDataManager.Verify(x => x.RequestSearchUpdateAsync(It.IsAny<string>(), "Test1", It.IsAny<SearchType>(), It.IsAny<RequestOptions>()), Times.Once);

        await cacheManager.Refresh(UpdateType.Search, stubSearch2.Object);

        // Now as we passed a different search object, the refresh should be queued.
        Assert.AreEqual(stubSearch2.Object, cacheManager.PendingSearch);
        Assert.AreEqual(cacheManager.PendingRefreshState, cacheManager.State);

        mockGitHubDataManager.Raise(x => x.OnUpdate += null, this, new DataManagerUpdateEventArgs(DataManagerUpdateKind.Cancel, UpdateType.Search, string.Empty, Array.Empty<string>()));

        Thread.Sleep(1000);
        Assert.AreEqual(cacheManager.RefreshingState, cacheManager.State);
        mockGitHubDataManager.Verify(x => x.RequestSearchUpdateAsync(It.IsAny<string>(), "Test2", It.IsAny<SearchType>(), It.IsAny<RequestOptions>()), Times.Once);

        mockGitHubDataManager.Raise(x => x.OnUpdate += null, this, new DataManagerUpdateEventArgs(DataManagerUpdateKind.Success, UpdateType.Search, string.Empty, Array.Empty<string>()));

        Thread.Sleep(1000);
        Assert.AreEqual(cacheManager.IdleState, cacheManager.State);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task CacheManagerRefreshCancellationTest()
    {
        var mockSearchHelper = MockSearchHelper();
        var mockRepositoryHelper = MockRepositoryHelper();
        var mockGitHubDataManager = MockGitHubDataManager();
        using var cacheManager = new CacheManager(mockGitHubDataManager.Object, mockRepositoryHelper.Object, mockSearchHelper.Object);

        Assert.AreEqual(cacheManager.IdleState, cacheManager.State);

        var stubSearch = new Mock<PersistentData.Search>();
        stubSearch.SetupAllProperties();
        await cacheManager.Refresh(UpdateType.Search, stubSearch.Object);

        Assert.AreEqual(cacheManager.RefreshingState, cacheManager.State);

        mockGitHubDataManager.Raise(x => x.OnUpdate += null, this, new DataManagerUpdateEventArgs(DataManagerUpdateKind.Cancel, UpdateType.All, string.Empty, Array.Empty<string>()));

        Thread.Sleep(1000);
        Assert.AreEqual(cacheManager.IdleState, cacheManager.State);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task CacheManagerPeriodicUpdateCancellationTest()
    {
        var mockSearchHelper = MockSearchHelper();
        var mockRepositoryHelper = MockRepositoryHelper();
        var mockGitHubDataManager = MockGitHubDataManager();
        using var cacheManager = new CacheManager(mockGitHubDataManager.Object, mockRepositoryHelper.Object, mockSearchHelper.Object);

        Assert.AreEqual(cacheManager.IdleState, cacheManager.State);
        await cacheManager.PeriodicUpdate();

        Assert.AreEqual(cacheManager.PeriodicUpdatingState, cacheManager.State);

        mockGitHubDataManager.Raise(x => x.OnUpdate += null, this, new DataManagerUpdateEventArgs(DataManagerUpdateKind.Cancel, UpdateType.All, string.Empty, Array.Empty<string>()));

        Thread.Sleep(1000);
        Assert.AreEqual(cacheManager.IdleState, cacheManager.State);
    }
}
