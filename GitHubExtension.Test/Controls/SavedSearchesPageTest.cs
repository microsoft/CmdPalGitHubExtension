// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.Controls.Pages;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Moq;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class SavedSearchesPageTest
{
    [TestMethod]
    [TestCategory("Unit")]
    public void SavedSearchesPageCreate()
    {
        var stubSearchPageFactory = new Mock<ISearchPageFactory>().Object;
        var stubSearchRepository = new Mock<ISearchRepository>().Object;
        var stubAddSearchListItem = new Mock<IListItem>().Object;
        var stubResources = new Mock<IResources>().Object;
        var savedSearchesPage = new SavedSearchesPage(stubSearchPageFactory, stubSearchRepository, stubResources, stubAddSearchListItem);
        Assert.IsNotNull(savedSearchesPage);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GetItemsFromSavedSearchesPage()
    {
        var stubSearchPageFactory = new Mock<ISearchPageFactory>();
        var stubSearchRepository = new Mock<ISearchRepository>();
        var stubAddSearchListItem = new Mock<IListItem>();
        var stubResources = new Mock<IResources>();
        var savedSearchesPage = new SavedSearchesPage(stubSearchPageFactory.Object, stubSearchRepository.Object, stubResources.Object, stubAddSearchListItem.Object);
        var savedSearches = new List<ISearch>
        {
            new Mock<ISearch>().Object,
            new Mock<ISearch>().Object,
        };
        stubSearchRepository.Setup(x => x.GetSavedSearches()).ReturnsAsync(savedSearches);
        var items = savedSearchesPage.GetItems();
        Assert.AreEqual(savedSearches.Count + 1, items.Length);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GetItemsFromSavedSearchesPageWhenNoSearches()
    {
        var stubSearchPageFactory = new Mock<ISearchPageFactory>();
        var stubSearchRepository = new Mock<ISearchRepository>();
        var stubAddSearchListItem = new Mock<IListItem>();
        var stubResources = new Mock<IResources>();
        var savedSearchesPage = new SavedSearchesPage(stubSearchPageFactory.Object, stubSearchRepository.Object, stubResources.Object, stubAddSearchListItem.Object);
        var items = savedSearchesPage.GetItems();
        Assert.AreEqual(1, items.Length);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void AddingSearchSaved()
    {
        var stubSearchPageFactory = new Mock<ISearchPageFactory>();
        var stubSearchRepository = new Mock<ISearchRepository>();
        var stubAddSearchListItem = new Mock<IListItem>();
        var stubResources = new Mock<IResources>();
        var savedSearchesPage = new SavedSearchesPage(stubSearchPageFactory.Object, stubSearchRepository.Object, stubResources.Object, stubAddSearchListItem.Object);

        savedSearchesPage.OnSearchSaved(this, new SearchCandidate());
        stubSearchRepository.Verify(x => x.GetSavedSearches(), Times.Once);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void RemoveSavedSearch()
    {
        var stubSearchPageFactory = new Mock<ISearchPageFactory>();
        var stubSearchRepository = new Mock<ISearchRepository>();
        var stubAddSearchListItem = new Mock<IListItem>();
        var stubResources = new Mock<IResources>();
        var savedSearchesPage = new SavedSearchesPage(stubSearchPageFactory.Object, stubSearchRepository.Object, stubResources.Object, stubAddSearchListItem.Object);
        var search = new Mock<ISearch>().Object;
        var savedSearchesPostRemove = new List<ISearch>();

        savedSearchesPage.OnSearchRemoved(this, true);
        stubSearchRepository.Verify(x => x.GetSavedSearches(), Times.Once);
        stubSearchRepository.Setup(x => x.GetSavedSearches()).ReturnsAsync(savedSearchesPostRemove);

        var items = savedSearchesPage.GetItems();
        Assert.AreEqual(savedSearchesPostRemove.Count + 1, items.Length); // +1 for the add search item
    }
}
