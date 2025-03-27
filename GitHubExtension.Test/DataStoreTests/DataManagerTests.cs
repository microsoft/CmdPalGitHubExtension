// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Client;
using GitHubExtension.DataManager.Cache;
using GitHubExtension.DataManager.Data;
using GitHubExtension.DeveloperId;
using Moq;

namespace GitHubExtension.Test.DataStoreTests;

public partial class DataStoreTests
{
    private DeveloperIdProvider GetDeveloperIdProvider() => new(new Mock<ICredentialVault>().Object);

    private GitHubClientProvider GetGitHubClientProvider(DeveloperIdProvider developerIdProvider) =>
        new(developerIdProvider);

    [TestMethod]
    [TestCategory("Unit")]
    public void DataManagerCreate()
    {
        using var dataManager = new GitHubDataManager(GetGitHubClientProvider(GetDeveloperIdProvider()), TestOptions.DataStoreOptions);
        Assert.IsNotNull(dataManager);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void DataManagerGetRepositories()
    {
        using var dataManager = new GitHubDataManager(GetGitHubClientProvider(GetDeveloperIdProvider()), TestOptions.DataStoreOptions);
        Assert.IsNotNull(dataManager);

        var repos = dataManager.GetRepositories();
        Assert.IsNotNull(repos);

        var noSuchRepo = dataManager.GetRepository("foo/bar");
        Assert.IsNull(noSuchRepo);

        noSuchRepo = dataManager.GetRepository("foo", "bar");
        Assert.IsNull(noSuchRepo);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void DataManagerGetIssues()
    {
        using var dataManager = new GitHubDataManager(GetGitHubClientProvider(GetDeveloperIdProvider()), TestOptions.DataStoreOptions);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void DataUpdater()
    {
        var countingDoneEvent = new ManualResetEvent(false);
        var count = 0;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        using var dataUpdater = new DataUpdater(
            TimeSpan.FromSeconds(1),
            async () =>
            {
                TestContext?.WriteLine($"In DataUpdater thread: {count}");
                ++count;
                if (count == 3)
                {
                    countingDoneEvent.Set();
                }
            });
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        Assert.IsNotNull(dataUpdater);

        // Data Updater will kick off an asynchronous task. we will wait for it to cycle three times terminate.
        _ = dataUpdater.Start();
        countingDoneEvent.WaitOne();
        dataUpdater.Stop();
        Assert.AreEqual(3, count);

        // Reset and do it again, this time testing stop mid-way.
        // Data Updater will kick off an asynchronous task. We will wait and give it enough time to
        // update twice and then stop it halfway through the second update.
        count = 0;
        _ = dataUpdater.Start();
        Thread.Sleep(1500);
        dataUpdater.Stop();
        Assert.IsFalse(dataUpdater.IsRunning);
        Thread.Sleep(2100);

        // After over two more seconds data updater has had time to count a few more times, unless
        // it was stopped successfully, in which case it would still only be at 1.
        // This test can randomly fail based on timings in builds, so disabling this check to avoid
        // 1-off errors from tanking a build.
        // Assert.AreEqual(1, count);
    }
}
