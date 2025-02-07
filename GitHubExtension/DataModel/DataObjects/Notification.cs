// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Dapper;
using Dapper.Contrib.Extensions;
using GitHubExtension.Helpers;
using Microsoft.Windows.ApplicationModel.Resources;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using Serilog;

namespace GitHubExtension.DataModel;

/// <summary>
/// Represents data for rendering a notification to the user.
/// </summary>
/// <remarks>
/// Notifications are sent to the user as Windows notifications, but may have specific filtering,
/// such as prioritizing certain repositories, or only showing a notification for a current
/// developer. The contents of the notifications table is a representation of potential
/// notifications, it does not necessarily mean the notification will be shown to the user. It is
/// the set of things we believe may be notification-worthy and ultimately user settings and context
/// will determine when and if the notification gets shown.
/// </remarks>
[Table("Notification")]
public class Notification
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", $"DataModel/{nameof(Notification)}"));

    private static readonly ILogger _log = _logger.Value;

    [Key]
    public long Id { get; set; } = DataStore.NoForeignKey;

    public long TypeId { get; set; } = DataStore.NoForeignKey;

    // User table - for filtering notifications by source.
    public long UserId { get; set; } = DataStore.NoForeignKey;

    // Repository table - for filtering notifications by source.
    public long RepositoryId { get; set; } = DataStore.NoForeignKey;

    public string Title { get; set; } = string.Empty;

    public string Identifier { get; set; } = string.Empty;

    public string Result { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string HtmlUrl { get; set; } = string.Empty;

    public string DetailsUrl { get; set; } = string.Empty;

    public long ToastState { get; set; } = DataStore.NoForeignKey;

    public long TimeOccurred { get; set; } = DataStore.NoForeignKey;

    public long TimeCreated { get; set; } = DataStore.NoForeignKey;

    [Write(false)]
    private DataStore? DataStore { get; set; }

    /// <summary>
    /// Gets the time this Notification was created.
    /// </summary>
    [Write(false)]
    [Computed]
    public DateTime CreatedAt => TimeCreated.ToDateTime();

    /// <summary>
    /// Gets the time the underlying event which this Notification represents occurred.
    /// </summary>
    [Write(false)]
    [Computed]
    public DateTime OccurredAt => TimeOccurred.ToDateTime();

    [Write(false)]
    [Computed]
    public NotificationType Type => (NotificationType)TypeId;

    [Write(false)]
    [Computed]
    public bool Toasted
    {
        get => ToastState != 0;
        set
        {
            ToastState = value ? 1 : 0;
            if (DataStore is not null)
            {
                try
                {
                    DataStore.Connection!.Update(this);
                }
                catch (Exception ex)
                {
                    // Catch errors so we do not throw for something like this. The local ToastState
                    // will still be set even if the datastore update fails. This could result in a
                    // toast later being shown twice, however, so report it as an error.
                    _log.Error(ex, "Failed setting Notification ToastState for Notification Id = {Id}");
                }
            }
        }
    }

    /// <summary>
    /// Gets the Repository of the content which created this Notification.
    /// </summary>
    [Write(false)]
    [Computed]
    public Repository Repository
    {
        get
        {
            if (DataStore == null)
            {
                return new Repository();
            }
            else
            {
                return Repository.GetById(DataStore, RepositoryId) ?? new Repository();
            }
        }
    }

    /// <summary>
    /// Gets the User of the content which created this Notification.
    /// </summary>
    [Write(false)]
    [Computed]
    public User User
    {
        get
        {
            if (DataStore == null)
            {
                return new User();
            }
            else
            {
                return User.GetById(DataStore, UserId) ?? new User();
            }
        }
    }

    public override string ToString() => $"[{Type}][{Repository}] {Title}";

    /// <summary>
    /// Shows a toast formatted based on this notification's NotificationType.
    /// </summary>
    /// <returns>True if a toast was shown.</returns>
    public bool ShowToast()
    {
        if (Toasted)
        {
            return false;
        }
        else if (LocalSettings.ReadSettingAsync<string>("NotificationsEnabled").Result == "false")
        {
            Toasted = true;
            return false;
        }

        return Type switch
        {
            NotificationType.CheckRunFailed => ShowFailedCheckRunToast(),
            NotificationType.CheckRunSucceeded => ShowSucceededCheckRunToast(),
            NotificationType.NewReview => ShowNewReviewToast(),
            _ => false,
        };
    }

    private bool ShowFailedCheckRunToast()
    {
        try
        {
            _log.Information($"Showing Notification for {this}");
            var resLoader = new ResourceLoader(ResourceLoader.GetDefaultResourceFilePath(), "GitHubExtension/Resources");
            var nb = new AppNotificationBuilder();
            nb.SetDuration(AppNotificationDuration.Long);
            nb.AddArgument("htmlurl", HtmlUrl);
            nb.AddText($"❌ {resLoader.GetString("Notifications_Toast_CheckRunFailed/Title")}");
            nb.AddText($"#{Identifier} - {Repository.FullName}", new AppNotificationTextProperties().SetMaxLines(1));

            // We want to show Author login but the AppNotification has a max 3 AddText calls, see:
            // https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.windows.appnotifications.builder.appnotificationbuilder.addtext?view=windows-app-sdk-1.2
            // The newline is a workaround to the 3 line restriction to show the Author line.
            nb.AddText(Title + Environment.NewLine + "@" + User.Login);
            nb.AddButton(new AppNotificationButton(resLoader.GetString("Notifications_Toast_Button/Dismiss")).AddArgument("action", "dismiss"));
            AppNotificationManager.Default.Show(nb.BuildNotification());

            Toasted = true;
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"Failed creating the Notification for {this}");
            return false;
        }

        return true;
    }

    private bool ShowSucceededCheckRunToast()
    {
        try
        {
            _log.Information($"Showing Notification for {this}");
            var resLoader = new ResourceLoader(ResourceLoader.GetDefaultResourceFilePath(), "GitHubExtension/Resources");
            var nb = new AppNotificationBuilder();
            nb.SetDuration(AppNotificationDuration.Long);
            nb.AddArgument("htmlurl", HtmlUrl);
            nb.AddText($"✅ {resLoader.GetString("Notifications_Toast_CheckRunSucceeded/Title")}");
            nb.AddText($"#{Identifier} - {Repository.FullName}", new AppNotificationTextProperties().SetMaxLines(1));

            // We want to show Author login but the AppNotification has a max 3 AddText calls, see:
            // https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.windows.appnotifications.builder.appnotificationbuilder.addtext?view=windows-app-sdk-1.2
            // The newline is a workaround to the 3 line restriction to show the Author line.
            nb.AddText(Title + Environment.NewLine + "@" + User.Login);
            nb.AddButton(new AppNotificationButton(resLoader.GetString("Notifications_Toast_Button/Dismiss")).AddArgument("action", "dismiss"));
            AppNotificationManager.Default.Show(nb.BuildNotification());

            Toasted = true;
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"Failed creating the Notification for {this}");
            return false;
        }

        return true;
    }

    private bool ShowNewReviewToast()
    {
        try
        {
            _log.Information($"Showing Notification for {this}");
            var resLoader = new ResourceLoader(ResourceLoader.GetDefaultResourceFilePath(), "GitHubExtension/Resources");
            var nb = new AppNotificationBuilder();
            nb.SetDuration(AppNotificationDuration.Long);
            nb.AddArgument("htmlurl", HtmlUrl);

            switch (Result)
            {
                case "Approved":
                    nb.AddText($"✅ {resLoader.GetString("Notifications_Toast_NewReview/Approved")}");
                    break;

                case "ChangesRequested":
                    nb.AddText($"⚠️ {resLoader.GetString("Notifications_Toast_NewReview/ChangesRequested")}");
                    break;

                default:
                    throw new ArgumentException($"Unknown Review Result: {Result}");
            }

            nb.AddText($"#{Identifier} - {Repository.FullName}", new AppNotificationTextProperties().SetMaxLines(1));

            // We want to show Author login but the AppNotification has a max 3 AddText calls, see:
            // https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.windows.appnotifications.builder.appnotificationbuilder.addtext?view=windows-app-sdk-1.2
            // The newline is a workaround to the 3 line restriction to show the Author line.
            nb.AddText(Title + Environment.NewLine + "@" + User.Login);
            nb.AddButton(new AppNotificationButton(resLoader.GetString("Notifications_Toast_Button/Dismiss")).AddArgument("action", "dismiss"));
            AppNotificationManager.Default.Show(nb.BuildNotification());
            Toasted = true;
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"Failed creating the Notification for {this}");
            return false;
        }

        return true;
    }

    public static Notification Create(DataStore dataStore, PullRequestStatus status, NotificationType type)
    {
        var pullRequestNotification = new Notification
        {
            TypeId = (long)type,
            UserId = status.PullRequest.AuthorId,
            RepositoryId = status.PullRequest.RepositoryId,
            Title = status.PullRequest.Title,
            Description = status.PullRequest.Body,
            Identifier = status.PullRequest.Number.ToStringInvariant(),
            Result = status.Conclusion.ToString(),
            HtmlUrl = status.PullRequest.HtmlUrl,
            DetailsUrl = status.DetailsUrl,
            ToastState = 0,
            TimeOccurred = status.TimeOccurred,
            TimeCreated = DateTime.Now.ToDataStoreInteger(),
        };

        Add(dataStore, pullRequestNotification);
        SetOlderNotificationsToasted(dataStore, pullRequestNotification);
        return pullRequestNotification;
    }

    public static Notification Create(DataStore dataStore, Review review, NotificationType type)
    {
        var reviewNotification = new Notification
        {
            TypeId = (long)type,
            UserId = review.AuthorId,
            RepositoryId = review.PullRequest.RepositoryId,
            Title = review.PullRequest.Title,
            Description = review.Body,
            Identifier = review.PullRequest.Number.ToStringInvariant(),
            Result = review.State,
            HtmlUrl = review.HtmlUrl,
            DetailsUrl = review.HtmlUrl,
            ToastState = 0,
            TimeOccurred = review.TimeSubmitted,
            TimeCreated = DateTime.Now.ToDataStoreInteger(),
        };

        Add(dataStore, reviewNotification);
        SetOlderNotificationsToasted(dataStore, reviewNotification);
        return reviewNotification;
    }

    public static Notification Add(DataStore dataStore, Notification notification)
    {
        notification.Id = dataStore.Connection!.Insert(notification);
        notification.DataStore = dataStore;
        return notification;
    }

    public static IEnumerable<Notification> Get(DataStore dataStore, DateTime? since = null, bool includeToasted = false)
    {
        since ??= DateTime.MinValue;
        var sql = @"SELECT * FROM Notification WHERE TimeCreated > @Time AND ToastState <= @ToastedCount ORDER BY TimeCreated DESC";
        var param = new
        {
            // Cast to non-nullable type since we ensure it is not null above.
            Time = ((DateTime)since).ToDataStoreInteger(),
            ToastedCount = includeToasted ? 1 : 0,
        };

        _log.Verbose(DataStore.GetSqlLogMessage(sql, param));
        var notifications = dataStore.Connection!.Query<Notification>(sql, param, null) ?? Enumerable.Empty<Notification>();
        foreach (var notification in notifications)
        {
            notification.DataStore = dataStore;
        }

        return notifications;
    }

    public static void SetOlderNotificationsToasted(DataStore dataStore, Notification notification)
    {
        // Get all untoasted notifications for the same type, identifier, and author that are older
        // than the specified notification.
        var sql = @"SELECT * FROM Notification WHERE TypeId = @TypeId AND RepositoryId = @RepositoryId AND Identifier = @Identifier AND UserId = @UserId AND TimeOccurred < @TimeOccurred AND ToastState = 0";
        var param = new
        {
            notification.TypeId,
            notification.RepositoryId,
            notification.Identifier,
            notification.UserId,
            notification.TimeOccurred,
        };

        _log.Verbose(DataStore.GetSqlLogMessage(sql, param));
        var outDatedNotifications = dataStore.Connection!.Query<Notification>(sql, param, null) ?? Enumerable.Empty<Notification>();
        foreach (var olderNotification in outDatedNotifications)
        {
            olderNotification.DataStore = dataStore;
            olderNotification.Toasted = true;
            _log.Information($"Found older notification for {olderNotification.Identifier} with result {olderNotification.Result}, marking toasted.");
        }
    }

    public static void DeleteBefore(DataStore dataStore, DateTime date)
    {
        // Delete notifications older than the date listed.
        var sql = @"DELETE FROM Notification WHERE TimeCreated < $Time;";
        var command = dataStore.Connection!.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$Time", date.ToDataStoreInteger());
        _log.Verbose(DataStore.GetCommandLogMessage(sql, command));
        var rowsDeleted = command.ExecuteNonQuery();
        _log.Verbose(DataStore.GetDeletedLogMessage(rowsDeleted));
    }
}
