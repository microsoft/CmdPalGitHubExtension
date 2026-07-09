// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls.Commands;
using GitHubExtension.DataManager;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Serilog;

namespace GitHubExtension.Controls.Pages;

public sealed partial class NotificationsPage : ListPage
{
    private readonly INotificationsDataManager _dataManager;
    private readonly NotificationsMediator _mediator;
    private readonly IResources _resources;
    private readonly ILogger _log;

    public int UnreadCount { get; private set; }

    public NotificationsPage(INotificationsDataManager dataManager, NotificationsMediator mediator, IResources resources)
    {
        _dataManager = dataManager;
        _mediator = mediator;
        _resources = resources;
        _log = Log.ForContext("SourceContext", $"Pages/{nameof(NotificationsPage)}");

        Name = resources.GetResource("Pages_Notifications");
        Icon = GitHubIcon.IconDictionary["Notifications"];

        _mediator.NotificationsChanged += OnNotificationsChanged;
    }

    private void OnNotificationsChanged(object? sender, object? args)
    {
        RaiseItemsChanged(0);
    }

    public override IListItem[] GetItems() => DoGetItems().GetAwaiter().GetResult();

    // Refreshes the unread count without rendering the list. Used to populate the
    // top-level command subtitle without requiring the user to open the page.
    public async Task RefreshUnreadCountAsync()
    {
        try
        {
            UnreadCount = await _dataManager.GetUnreadCountAsync();
            _mediator.NotifyNotificationsChanged();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to refresh unread notification count.");
        }
    }

    private async Task<IListItem[]> DoGetItems()
    {
        try
        {
            var notifications = (await _dataManager.GetNotificationsAsync(true)).ToList();
            UnreadCount = notifications.Count(n => n.Unread);

            if (notifications.Count == 0)
            {
                return new IListItem[]
                {
                    new ListItem(new NoOpCommand())
                    {
                        Title = _resources.GetResource("Pages_Notifications_None"),
                        Icon = GitHubIcon.IconDictionary["Notifications"],
                    },
                };
            }

            return notifications.Select(GetListItem).ToArray();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to load notifications.");
            return new IListItem[]
            {
                new ListItem(new NoOpCommand())
                {
                    Title = _resources.GetResource("Pages_Error_Title"),
                    Details = new Details()
                    {
                        Title = ex.Message,
                        Body = string.IsNullOrEmpty(ex.StackTrace) ? "There is no stack trace for the error." : ex.StackTrace,
                    },
                },
            };
        }
    }

    private ListItem GetListItem(INotification notification)
    {
        return new ListItem(new LinkCommand(notification, _resources))
        {
            Title = notification.Title,
            Icon = GitHubIcon.IconDictionary["Notifications"],
            Subtitle = notification.RepositoryFullName,
            MoreCommands = new CommandContextItem[]
            {
                new(new MarkNotificationReadCommand(notification, _dataManager, _mediator, _resources)),
                new(new CopyCommand(notification.HtmlUrl, $"{_resources.GetResource("Commands_CopyURL")}", _resources)),
            },
        };
    }
}
