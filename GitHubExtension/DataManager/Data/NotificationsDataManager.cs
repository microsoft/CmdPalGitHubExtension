// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Client;
using GitHubExtension.Controls;
using GitHubExtension.Helpers;
using Octokit;
using Serilog;

namespace GitHubExtension.DataManager.Data;

public sealed class NotificationsDataManager : INotificationsDataManager
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", nameof(NotificationsDataManager)));

    private static readonly ILogger _log = _logger.Value;

    private readonly GitHubClientProvider _gitHubClientProvider;

    public NotificationsDataManager(GitHubClientProvider gitHubClientProvider)
    {
        _gitHubClientProvider = gitHubClientProvider;
    }

    public async Task<IEnumerable<INotification>> GetNotificationsAsync(bool onlyUnread = true)
    {
        var client = await _gitHubClientProvider.GetClientForLoggedInDeveloper(false);

        var request = new NotificationsRequest { All = !onlyUnread };
        var notifications = await client.Activity.Notifications.GetAllForCurrent(request);

        _log.Debug($"Retrieved {notifications.Count} notifications (onlyUnread: {onlyUnread}).");

        return notifications.Select(ToNotification).ToList();
    }

    public async Task MarkNotificationAsReadAsync(string id)
    {
        if (!int.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var threadId))
        {
            _log.Warning($"Unable to parse notification id '{id}' as a thread id.");
            return;
        }

        var client = await _gitHubClientProvider.GetClientForLoggedInDeveloper(false);
        await client.Activity.Notifications.MarkAsRead(threadId);
        _log.Information($"Marked notification {threadId} as read.");
    }

    public async Task<int> GetUnreadCountAsync()
    {
        var notifications = await GetNotificationsAsync(true);
        return notifications.Count(n => n.Unread);
    }

    private static Controls.Notification ToNotification(Octokit.Notification notification)
    {
        var repositoryFullName = notification.Repository?.FullName ?? string.Empty;
        var repositoryHtmlUrl = notification.Repository?.HtmlUrl ?? string.Empty;

        return new Controls.Notification
        {
            Id = notification.Id ?? string.Empty,
            Title = notification.Subject?.Title ?? string.Empty,
            Reason = notification.Reason ?? string.Empty,
            RepositoryFullName = repositoryFullName,
            Type = notification.Subject?.Type ?? string.Empty,
            HtmlUrl = NotificationHelper.GetHtmlUrl(notification.Subject?.Url, notification.Subject?.Type, repositoryHtmlUrl),
            Unread = notification.Unread,
            UpdatedAt = ParseDate(notification.UpdatedAt),
        };
    }

    private static DateTimeOffset ParseDate(string? value)
    {
        if (!string.IsNullOrEmpty(value) &&
            DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed))
        {
            return parsed;
        }

        return DateTimeOffset.MinValue;
    }
}
