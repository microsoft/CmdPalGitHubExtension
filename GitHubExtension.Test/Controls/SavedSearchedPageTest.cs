// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.Controls.ListItems;
using GitHubExtension.Controls.Pages;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Moq;
using Windows.ApplicationModel.VoiceCommands;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class SavedSearchedPageTest
{
    [TestMethod]
    [TestCategory("Unit")]
    public void SavedSearchesPageCreate()
    {
        var stubSearchPageFactory = new Mock<ISearchPageFactory>().Object;
        var stubSearchRepository = new Mock<ISearchRepository>().Object;
        var stubAddSearchListItem = new Mock<IListItem>().Object;
        var stubAddSearchFullFormListItem = new Mock<IListItem>().Object;
        var savedSearchesPage = new SavedSearchesPage(stubSearchPageFactory, stubSearchRepository, stubAddSearchListItem, stubAddSearchFullFormListItem);
        Assert.IsNotNull(savedSearchesPage);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GetItemsFromSavedSearchesPage()
    {
        var stubSearchPageFactory = new Mock<ISearchPageFactory>();
        var stubSearchRepository = new Mock<ISearchRepository>();
        var stubAddSearchListItem = new Mock<IListItem>();
        var stubAddSearchFullFormListItem = new Mock<IListItem>();
        var savedSearchesPage = new SavedSearchesPage(stubSearchPageFactory.Object, stubSearchRepository.Object, stubAddSearchListItem.Object, stubAddSearchFullFormListItem.Object);
        var savedSearches = new List<ISearch>
        {
            new Mock<ISearch>().Object,
            new Mock<ISearch>().Object,
        };
        stubSearchRepository.Setup(x => x.GetSavedSearches()).ReturnsAsync(savedSearches);
        var items = savedSearchesPage.GetItems();
        Assert.AreEqual(savedSearches.Count + 2, items.Length);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GetItemsFromSavedSearchesPageWhenNoSearches()
    {
        var stubSearchPageFactory = new Mock<ISearchPageFactory>();
        var stubSearchRepository = new Mock<ISearchRepository>();
        var stubAddSearchListItem = new Mock<IListItem>();
        var stubAddSearchFullFormListItem = new Mock<IListItem>();
        var savedSearchesPage = new SavedSearchesPage(stubSearchPageFactory.Object, stubSearchRepository.Object, stubAddSearchListItem.Object, stubAddSearchFullFormListItem.Object);
        var items = savedSearchesPage.GetItems();
        Assert.AreEqual(2, items.Length);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void AddingSearchSaved()
    {
        var stubSearchPageFactory = new Mock<ISearchPageFactory>();
        var stubSearchRepository = new Mock<ISearchRepository>();
        var stubAddSearchListItem = new Mock<IListItem>();
        var stubAddSearchFullFormListItem = new Mock<IListItem>();
        var savedSearchesPage = new SavedSearchesPage(stubSearchPageFactory.Object, stubSearchRepository.Object, stubAddSearchListItem.Object, stubAddSearchFullFormListItem.Object);

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
        var stubAddSearchFullFormListItem = new Mock<IListItem>();
        var savedSearchesPage = new SavedSearchesPage(stubSearchPageFactory.Object, stubSearchRepository.Object, stubAddSearchListItem.Object, stubAddSearchFullFormListItem.Object);
        var search = new Mock<ISearch>().Object;

        savedSearchesPage.OnSearchRemoved(this, true);
        stubSearchRepository.Verify(x => x.GetSavedSearches(), Times.Once);
    }
}
