// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.Controls;

public class NotificationsMediator
{
    public event EventHandler<object?>? NotificationsChanged;

    public NotificationsMediator()
    {
    }

    public void NotifyNotificationsChanged(object? args = null)
    {
        NotificationsChanged?.Invoke(this, args);
    }
}
