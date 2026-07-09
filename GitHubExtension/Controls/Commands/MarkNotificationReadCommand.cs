// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.DataManager;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Serilog;

namespace GitHubExtension.Controls.Commands;

internal sealed partial class MarkNotificationReadCommand : InvokableCommand
{
    private static readonly ILogger _log = Log.ForContext("SourceContext", nameof(MarkNotificationReadCommand));

    private readonly INotification _notification;
    private readonly INotificationsDataManager _dataManager;
    private readonly NotificationsMediator _mediator;
    private readonly IResources _resources;

    internal MarkNotificationReadCommand(INotification notification, INotificationsDataManager dataManager, NotificationsMediator mediator, IResources resources)
    {
        _notification = notification;
        _dataManager = dataManager;
        _mediator = mediator;
        _resources = resources;
        Name = resources.GetResource("Commands_MarkNotificationRead");
        Icon = new IconInfo("\uE8FB");
    }

    public override CommandResult Invoke()
    {
        try
        {
            _dataManager.MarkNotificationAsReadAsync(_notification.Id).GetAwaiter().GetResult();
            ToastHelper.ShowSuccessToast(_resources.GetResource("Message_MarkNotificationRead_Success"));
            _mediator.NotifyNotificationsChanged();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to mark notification as read.");
            ToastHelper.ShowErrorToast(_resources.GetResource("Message_MarkNotificationRead_Error"));
        }

        return CommandResult.KeepOpen();
    }
}
