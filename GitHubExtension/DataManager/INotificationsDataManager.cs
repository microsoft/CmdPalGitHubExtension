// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;

namespace GitHubExtension.DataManager;

public interface INotificationsDataManager
{
    // Fetches notifications for the signed-in user. When onlyUnread is true only
    // unread notifications are returned (GitHub's default behavior).
    Task<IEnumerable<INotification>> GetNotificationsAsync(bool onlyUnread = true);

    // Marks a single notification thread as read.
    Task MarkNotificationAsReadAsync(string id);

    // Returns the number of unread notifications.
    Task<int> GetUnreadCountAsync();
}
