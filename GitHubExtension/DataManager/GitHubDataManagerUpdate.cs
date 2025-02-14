// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataManager;
using GitHubExtension.DataModel;

namespace GitHubExtension;

public partial class GitHubDataManager
{
    // This is how frequently the DataStore update occurs.
    private static readonly TimeSpan _updateInterval = TimeSpan.FromMinutes(5);
    private static DateTime _lastUpdateTime = DateTime.MinValue;

    public static async Task Update()
    {
        // Only update per the update interval.
        // This is intended to be dynamic in the future.
        if (DateTime.Now - _lastUpdateTime < _updateInterval)
        {
            return;
        }

        try
        {
            await UpdateDeveloperPullRequests();
        }
        catch (HttpRequestException httpEx)
        {
            // HttpRequestExceptions can happen when internet connection is
            // down or various other network issues unrelated to this update.
            // This is not an error in the extension or anything we can
            // address. Log a warning so it is understood why the update did
            // not occur, but otherwise keep the log clean.
            _log.Warning($"Http Request Exception: {httpEx.Message}");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Update failed unexpectedly.");
        }

        _lastUpdateTime = DateTime.Now;
    }

    private static async Task UpdateDeveloperPullRequests()
    {
        _log.Debug($"Executing UpdateDeveloperPullRequests");
        using var dataManager = CreateInstance() ?? throw new DataStoreInaccessibleException("GitHubDataManager is null.");
        await dataManager.UpdatePullRequestsForLoggedInDeveloperIdsAsync();

        // Show any new notifications that were created from the pull request update.
        var notifications = dataManager.GetNotifications();
        foreach (var notification in notifications)
        {
            // Show notifications for failed checkruns for Developer users.
            if (notification.Type == NotificationType.CheckRunFailed && notification.User.IsDeveloper)
            {
                notification.ShowToast();
            }

            // Show notifications for new reviews.
            if (notification.Type == NotificationType.NewReview)
            {
                notification.ShowToast();
            }
        }
    }

    private static void SendDeveloperUpdateEvent(object? source)
    {
        SendUpdateEvent(source, DataManagerUpdateKind.Developer, null, null);
    }

    private static void SendRepositoryUpdateEvent(object? source, string fullName, string[] context)
    {
        SendUpdateEvent(source, DataManagerUpdateKind.Repository, fullName, context);
    }

    private static void SendUpdateEvent(object? source, DataManagerUpdateKind kind, string? info = null, string[]? context = null)
    {
        if (OnUpdate != null)
        {
            info ??= string.Empty;
            context ??= Array.Empty<string>();
            _log.Information($"Sending Update Event: {kind}  Info: {info}  Context: {string.Join(",", context)}");
            OnUpdate.Invoke(source, new DataManagerUpdateEventArgs(kind, info, context));
        }
    }

    private static void SendPullRequestsUpdateEvent(object? source)
    {
        SendUpdateEvent(source, DataManagerUpdateKind.PullRequests, null, null);
    }

    private static void SendIssuesUpdateEvent(object? source)
    {
        SendUpdateEvent(source, DataManagerUpdateKind.Issues, null, null);
    }

    private static void SendAllDataUpdateEvent(object? source)
    {
        SendUpdateEvent(source, DataManagerUpdateKind.All, null, null);
    }

    private static void SendSearchUpdateEvent(object? source, string name)
    {
        SendUpdateEvent(source, DataManagerUpdateKind.Search, name, null);
    }

    private static void SendCancelUpdateEvent(object? source)
    {
        SendUpdateEvent(source, DataManagerUpdateKind.Cancel, null, null);
    }
}
