// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.Controls.Pages;
using GitHubExtension.DataManager;
using GitHubExtension.Helpers;
using Moq;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class NotificationsPageTests
{
    private (Mock<INotificationsDataManager> DataManager, NotificationsMediator Mediator, Mock<IResources> Resources) CreateCommonMocks()
    {
        var dataManager = new Mock<INotificationsDataManager>();
        var mediator = new NotificationsMediator();
        var resources = new Mock<IResources>();
        resources.Setup(x => x.GetResource(It.IsAny<string>(), null)).Returns("Mocked Resource");
        return (dataManager, mediator, resources);
    }

    private static Mock<INotification> CreateNotification(string id, string title, string repo, bool unread)
    {
        var notification = new Mock<INotification>();
        notification.Setup(x => x.Id).Returns(id);
        notification.Setup(x => x.Title).Returns(title);
        notification.Setup(x => x.RepositoryFullName).Returns(repo);
        notification.Setup(x => x.HtmlUrl).Returns($"https://github.com/{repo}");
        notification.Setup(x => x.Unread).Returns(unread);
        return notification;
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void NotificationsPage_Create_Succeeds()
    {
        var (dataManager, mediator, resources) = CreateCommonMocks();
        var page = new NotificationsPage(dataManager.Object, mediator, resources.Object);
        Assert.IsNotNull(page);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GetItems_ReturnsNotifications()
    {
        var (dataManager, mediator, resources) = CreateCommonMocks();
        var notifications = new List<INotification>
        {
            CreateNotification("1", "First", "owner/repo1", true).Object,
            CreateNotification("2", "Second", "owner/repo2", true).Object,
        };
        dataManager.Setup(x => x.GetNotificationsAsync(true)).ReturnsAsync(notifications);

        var page = new NotificationsPage(dataManager.Object, mediator, resources.Object);
        var items = page.GetItems();

        Assert.AreEqual(2, items.Length);
        Assert.AreEqual("First", items[0].Title);
        Assert.AreEqual("Second", items[1].Title);
        Assert.AreEqual(2, page.UnreadCount);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GetItems_NoNotifications_ReturnsPlaceholder()
    {
        var (dataManager, mediator, resources) = CreateCommonMocks();
        dataManager.Setup(x => x.GetNotificationsAsync(true)).ReturnsAsync(new List<INotification>());

        var page = new NotificationsPage(dataManager.Object, mediator, resources.Object);
        var items = page.GetItems();

        Assert.AreEqual(1, items.Length);
        Assert.AreEqual(0, page.UnreadCount);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task RefreshUnreadCountAsync_UpdatesCount()
    {
        var (dataManager, mediator, resources) = CreateCommonMocks();
        dataManager.Setup(x => x.GetUnreadCountAsync()).ReturnsAsync(5);

        var page = new NotificationsPage(dataManager.Object, mediator, resources.Object);
        await page.RefreshUnreadCountAsync();

        Assert.AreEqual(5, page.UnreadCount);
    }
}
