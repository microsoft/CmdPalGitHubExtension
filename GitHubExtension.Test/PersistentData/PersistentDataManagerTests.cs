// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataModel;
using GitHubExtension.DataModel.Enums;
using GitHubExtension.DeveloperId;
using GitHubExtension.Pages;
using GitHubExtension.PersistentData;
using Moq;
using Search = GitHubExtension.PersistentData.Search;

namespace GitHubExtension.Test.PersistentData;

[TestClass]
public partial class PersistentDataManagerTests
{
    // We can mock this.
    // This is used for validating a search.
    private DeveloperIdProvider GetDeveloperIdProvider() => new();

    [TestMethod]
    [TestCategory("Unit")]
    public void PeristentDataManagerCreate()
    {
        var dataStoreOptions = GetDataStoreOptions();
        using var dataManager = new PersistentDataManager(GetDeveloperIdProvider(), dataStoreOptions);
        Assert.IsNotNull(dataManager);
        dataManager.Dispose();
        Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task PersistentDataManagerAddAndRemoveSearch()
    {
        var dataStoreOptions = GetDataStoreOptions();
        using var dataManager = new PersistentDataManager(GetDeveloperIdProvider(), dataStoreOptions);

        var stubSearch = new Mock<ISearch>();
        stubSearch.SetupGet(x => x.Name).Returns("TestSearch");
        stubSearch.SetupGet(x => x.SearchString).Returns("test is:issue");
        stubSearch.SetupGet(x => x.Type).Returns(SearchType.Issues);

        await dataManager.AddSavedSearch(stubSearch.Object);

        var searches = await dataManager.GetSavedSearches();
        Assert.IsTrue(searches.Any());
        var dmSearch = dataManager.GetSearch(stubSearch.Object.Name, stubSearch.Object.SearchString);

        Assert.AreEqual(stubSearch.Object.Name, dmSearch.Name);
        Assert.AreEqual(stubSearch.Object.SearchString, dmSearch.SearchString);
        Assert.AreEqual(stubSearch.Object.Type, dmSearch.Type);

        await dataManager.RemoveSavedSearch(stubSearch.Object);

        Assert.IsFalse((await dataManager.GetSavedSearches()).Any());
        dataManager.Dispose();
        Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task PersistentDataManagerAddRepeatedSearch()
    {
        var dataStoreOptions = GetDataStoreOptions();
        using var dataManager = new PersistentDataManager(GetDeveloperIdProvider(), dataStoreOptions);

        var stubSearch = new Mock<ISearch>();
        stubSearch.SetupGet(x => x.Name).Returns("TestSearch");
        stubSearch.SetupGet(x => x.SearchString).Returns("test is:issue");
        stubSearch.SetupGet(x => x.Type).Returns(SearchType.Issues);

        await dataManager.AddSavedSearch(stubSearch.Object);

        var searches = await dataManager.GetSavedSearches();
        Assert.IsTrue(searches.Any());

        try
        {
            await dataManager.AddSavedSearch(stubSearch.Object);
        }
        catch (InvalidOperationException)
        {
            // Expected. We tried to add repeated search.
        }
        catch (Exception)
        {
            Assert.Fail("Should have failed.");
        }

        dataManager.Dispose();
        Cleanup(dataStoreOptions.DataStoreFolderPath);
    }
}
