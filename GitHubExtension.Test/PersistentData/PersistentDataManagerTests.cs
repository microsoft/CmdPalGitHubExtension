// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.DataModel.Enums;
using GitHubExtension.PersistentData;
using Moq;

namespace GitHubExtension.Test.PersistentData;

[TestClass]
public partial class PersistentDataManagerTests
{
    [TestMethod]
    [TestCategory("Unit")]
    public void Create()
    {
        var stubValidator = new Mock<IGitHubValidator>().Object;
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        using var dataManager = new PersistentDataManager(stubValidator, dataStoreOptions);
        Assert.IsNotNull(dataManager);
        dataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task AddAndRemoveSearch()
    {
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        var stubValidator = new Mock<IGitHubValidator>().Object;

        using var dataManager = new PersistentDataManager(stubValidator, dataStoreOptions);

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
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task AddRepeatedSearch()
    {
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        var stubValidator = new Mock<IGitHubValidator>().Object;

        using var dataManager = new PersistentDataManager(stubValidator, dataStoreOptions);

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
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task UpdateSearchStatus()
    {
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        var stubValidator = new Mock<IGitHubValidator>().Object;

        using var dataManager = new PersistentDataManager(stubValidator, dataStoreOptions);

        var stubSearch = new Mock<ISearch>();
        stubSearch.SetupGet(x => x.Name).Returns("TestSearch");
        stubSearch.SetupGet(x => x.SearchString).Returns("test is:issue");
        stubSearch.SetupGet(x => x.Type).Returns(SearchType.Issues);

        await dataManager.AddSavedSearch(stubSearch.Object);

        var searches = await dataManager.GetSavedSearches();
        Assert.IsTrue(searches.Any());
        var topLevelSearches = await dataManager.GetTopLevelSearches();
        Assert.IsFalse(topLevelSearches.Any());

        await dataManager.UpdateSearchTopLevelStatus(stubSearch.Object, true);
        topLevelSearches = await dataManager.GetTopLevelSearches();
        Assert.IsTrue(topLevelSearches.Any());
        Assert.AreEqual("TestSearch", topLevelSearches.ToList()[0].Name);
        Assert.IsTrue(await dataManager.IsTopLevel(stubSearch.Object));

        await dataManager.UpdateSearchTopLevelStatus(stubSearch.Object, false);
        topLevelSearches = await dataManager.GetTopLevelSearches();
        Assert.IsFalse(topLevelSearches.Any());

        dataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task AddTopLevelSearch()
    {
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        var stubValidator = new Mock<IGitHubValidator>().Object;

        using var dataManager = new PersistentDataManager(stubValidator, dataStoreOptions);

        var stubSearch = new Mock<ISearch>();
        stubSearch.SetupGet(x => x.Name).Returns("TestSearch");
        stubSearch.SetupGet(x => x.SearchString).Returns("test is:issue");
        stubSearch.SetupGet(x => x.Type).Returns(SearchType.Issues);

        await dataManager.UpdateSearchTopLevelStatus(stubSearch.Object, true);
        var topLevelSearches = await dataManager.GetTopLevelSearches();
        Assert.IsTrue(topLevelSearches.Any());
        Assert.AreEqual("TestSearch", topLevelSearches.ToList()[0].Name);

        Assert.IsTrue(await dataManager.IsTopLevel(stubSearch.Object));

        await dataManager.UpdateSearchTopLevelStatus(stubSearch.Object, false);
        topLevelSearches = await dataManager.GetTopLevelSearches();
        Assert.IsFalse(topLevelSearches.Any());

        dataManager.Dispose();
        PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task InitializeTopLevelSearches()
    {
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        var stubValidator = new Mock<IGitHubValidator>().Object;

        using var dataManager = new PersistentDataManager(stubValidator, dataStoreOptions);

        var stubSearches = new List<Mock<ISearch>>
        {
            new(),
            new(),
            new(),
        };

        for (var i = 0; i < stubSearches.Count; i++)
        {
            stubSearches[i].SetupGet(x => x.Name).Returns($"TestSearch{i}");
            stubSearches[i].SetupGet(x => x.SearchString).Returns($"test{i} is:issue");
            stubSearches[i].SetupGet(x => x.Type).Returns(SearchType.Issues);
        }

        var stubSearchesObjs = stubSearches.Select(x => x.Object).ToList();

        await dataManager.InitializeTopLevelSearches(stubSearchesObjs);

        var topLevelSearches = await dataManager.GetTopLevelSearches();

        Assert.AreEqual(stubSearchesObjs.Count, topLevelSearches.Count());

        for (var i = 0; i < stubSearches.Count; i++)
        {
            Assert.AreEqual(stubSearchesObjs[i].Name, topLevelSearches.ElementAt(i).Name);
            Assert.AreEqual(stubSearchesObjs[i].SearchString, topLevelSearches.ElementAt(i).SearchString);
            Assert.AreEqual(stubSearchesObjs[i].Type, topLevelSearches.ElementAt(i).Type);
        }
    }
}
