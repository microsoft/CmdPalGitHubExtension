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
    private (Mock<ISearchPageFactory> SearchPageFactory, Mock<ISearchRepository> SearchRepository, Mock<IListItem> AddSearchListItem, Mock<IResources> Resources, SavedSearchesMediator Mediator) CreateMocks()
    {
        var searchPageFactory = new Mock<ISearchPageFactory>();
        var searchRepository = new Mock<ISearchRepository>();
        var addSearchListItem = new Mock<IListItem>();
        var resources = new Mock<IResources>();
        var mediator = new SavedSearchesMediator();
        return (searchPageFactory, searchRepository, addSearchListItem, resources, mediator);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void SavedSearchesPageCreate_NotNull()
    {
        var (searchPageFactory, searchRepository, addSearchListItem, resources, mediator) = CreateMocks();
        var savedSearchesPage = new SavedSearchesPage(searchPageFactory.Object, searchRepository.Object, resources.Object, addSearchListItem.Object, mediator);
        Assert.IsNotNull(savedSearchesPage);
    }

    [DataRow(2, 3)]
    [DataRow(0, 1)]
    [TestMethod]
    [TestCategory("Unit")]
    public void GetItemsFromSavedSearchesPage_ReturnsExpectedCount(int searchCount, int expectedItemCount)
    {
        var (searchPageFactory, searchRepository, addSearchListItem, resources, mediator) = CreateMocks();
        var savedSearchesPage = new SavedSearchesPage(searchPageFactory.Object, searchRepository.Object, resources.Object, addSearchListItem.Object, mediator);

        var savedSearches = new List<ISearch>();
        for (var i = 0; i < searchCount; i++)
        {
            savedSearches.Add(new Mock<ISearch>().Object);
        }

        searchRepository.Setup(x => x.GetSavedSearches()).ReturnsAsync(savedSearches);

        var items = savedSearchesPage.GetItems();
        Assert.AreEqual(expectedItemCount, items.Length);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void RemoveSavedSearch_RemovesAndUpdatesItems()
    {
        var (searchPageFactory, searchRepository, addSearchListItem, resources, mediator) = CreateMocks();
        var savedSearchesPage = new SavedSearchesPage(searchPageFactory.Object, searchRepository.Object, resources.Object, addSearchListItem.Object, mediator);
        var search = new Mock<ISearch>().Object;
        var savedSearchesPostRemove = new List<ISearch>();

        var mockArgs = new SavedSearchRemovedEventArgs(true, null, search);
        mediator.RemoveSearch(mockArgs);
        searchRepository.Setup(x => x.GetSavedSearches()).ReturnsAsync(savedSearchesPostRemove);

        var items = savedSearchesPage.GetItems();
        Assert.AreEqual(1, items.Length); // Only the add search item remains
    }
}
