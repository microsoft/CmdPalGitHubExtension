// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Globalization;
using GitHubExtension.Controls.Commands;
using GitHubExtension.DataModel.DataObjects;
using GitHubExtension.Helpers;
using GitHubExtension.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Serilog;

namespace GitHubExtension.Controls.Pages;

public sealed partial class GitHubCopilotViewPage : ListPage, IDisposable
{
    private readonly IResources _resources;
    private readonly IGitHubCopilotService _copilotService;
    private readonly ILogger _logger;
    private IEnumerable<CopilotTask>? _cachedTasks;
    private DateTime _lastFetchTime = DateTime.MinValue;
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

    public GitHubCopilotViewPage(IResources resources, IGitHubCopilotService copilotService)
    {
        _resources = resources;
        _copilotService = copilotService;
        _logger = Log.ForContext("SourceContext", nameof(GitHubCopilotViewPage));

        Icon = GitHubIcon.IconDictionary["logo"];
        Name = _resources.GetResource("Commands_GitHub_Copilot_ViewTask");
    }

    public override IListItem[] GetItems()
    {
        try
        {
            if (!_copilotService.IsAvailable())
            {
                _logger.Warning("Copilot service not available - user not signed in");
                return new IListItem[]
                {
                    new ListItem(new Microsoft.CommandPalette.Extensions.Toolkit.NoOpCommand())
                    {
                        Title = "GitHub Sign In Required",
                        Subtitle = "Please sign in to GitHub to view Copilot tasks",
                        Icon = GitHubIcon.IconDictionary["logo"],
                    },
                };
            }

            _logger.Information("Fetching Copilot tasks...");
            var tasks = GetTasksSync();

            _logger.Information($"Retrieved {tasks.Count()} tasks from service");

            if (!tasks.Any())
            {
                _logger.Information("No tasks found - displaying informational message");
                return new IListItem[]
                {
                    new ListItem(new Microsoft.CommandPalette.Extensions.Toolkit.NoOpCommand())
                    {
                        Title = "No Copilot tasks found",
                        Subtitle = "No pull requests authored by GitHub Copilot were found. Try creating one in GitHub Copilot Workspace.",
                        Icon = GitHubIcon.IconDictionary["logo"],
                    },
                };
            }

            var listItems = new List<IListItem>();

            foreach (var task in tasks)
            {
                var statusIcon = GetStatusIcon(task.Status);
                var statusText = GetStatusText(task.Status);
                var timeText = GetRelativeTimeText(task.UpdatedAt);

                // Build comprehensive subtitle with all available info
                var subtitleParts = new List<string>
                {
                    statusText,
                    task.Agent,
                };

                if (!string.IsNullOrEmpty(task.Repository))
                {
                    subtitleParts.Add(task.Repository);
                }

                subtitleParts.Add(timeText);

                var listItem = new ListItem(new OpenTaskUrlCommand(task.Url, _resources))
                {
                    Title = task.Title,
                    Subtitle = string.Join(" • ", subtitleParts),
                    Icon = statusIcon,
                    MoreCommands = new CommandContextItem[]
                    {
                        new CommandContextItem(new CopilotTaskDetailPage(_resources, task))
                        {
                            Title = _resources.GetResource("Commands_View_Task_Details"),
                            Icon = new IconInfo("\uE946"), // Info icon
                        },
                    },
                };

                _logger.Debug($"Added task: {task.Title} (Status: {statusText})");
                listItems.Add(listItem);
            }

            _logger.Information($"Displaying {listItems.Count} Copilot tasks");
            return listItems.ToArray();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error loading Copilot tasks in GetItems");
            return new IListItem[]
            {
                new ListItem(new Microsoft.CommandPalette.Extensions.Toolkit.NoOpCommand())
                {
                    Title = "Error loading tasks",
                    Subtitle = $"An error occurred: {ex.Message}. Check logs for details.",
                    Icon = GitHubIcon.IconDictionary["logo"],
                },
            };
        }
    }

    private IEnumerable<CopilotTask> GetTasksSync()
    {
        try
        {
            // Use cached tasks if they're still fresh
            if (_cachedTasks != null && DateTime.UtcNow - _lastFetchTime < CacheExpiration)
            {
                _logger.Information($"Using cached tasks (age: {(DateTime.UtcNow - _lastFetchTime).TotalMinutes:F1} minutes)");
                return _cachedTasks;
            }

            _logger.Information("Cache expired or empty, fetching fresh tasks...");

            // This is not ideal, but GetItems() is synchronous and we need the data
            // We use Task.Run to avoid potential deadlocks
            var tasks = Task.Run(async () => await _copilotService.GetCopilotTasksAsync()).GetAwaiter().GetResult();

            _cachedTasks = tasks;
            _lastFetchTime = DateTime.UtcNow;

            _logger.Information($"Fetched and cached {tasks.Count()} tasks");
            return tasks;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving Copilot tasks synchronously");

            // Return cached tasks if available, even if expired
            if (_cachedTasks != null)
            {
                _logger.Warning("Returning stale cached tasks due to fetch error");
                return _cachedTasks;
            }

            return Enumerable.Empty<CopilotTask>();
        }
    }

    private static IconInfo GetStatusIcon(CopilotTaskStatus status)
    {
        return status switch
        {
            CopilotTaskStatus.Completed => new IconInfo("\uE73E"), // Checkmark
            CopilotTaskStatus.InProgress => new IconInfo("\uE9F3"), // Clock/Progress
            CopilotTaskStatus.Failed => new IconInfo("\uE783"), // Error/X
            CopilotTaskStatus.Cancelled => new IconInfo("\uE711"), // Cancel
            _ => new IconInfo("\uE946"), // Info/Default
        };
    }

    private static string GetStatusText(CopilotTaskStatus status)
    {
        return status switch
        {
            CopilotTaskStatus.Completed => "Completed",
            CopilotTaskStatus.InProgress => "In Progress",
            CopilotTaskStatus.Failed => "Failed",
            CopilotTaskStatus.Cancelled => "Cancelled",
            _ => "Unknown",
        };
    }

    private static string GetRelativeTimeText(DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;

        if (timeSpan.TotalMinutes < 1)
        {
            return "Just now";
        }

        if (timeSpan.TotalMinutes < 60)
        {
            return $"{(int)timeSpan.TotalMinutes}m ago";
        }

        if (timeSpan.TotalHours < 24)
        {
            return $"{(int)timeSpan.TotalHours}h ago";
        }

        if (timeSpan.TotalDays < 7)
        {
            return $"{(int)timeSpan.TotalDays}d ago";
        }

        if (timeSpan.TotalDays < 30)
        {
            return $"{(int)(timeSpan.TotalDays / 7)}w ago";
        }

        return dateTime.ToString("MMM dd, yyyy", CultureInfo.InvariantCulture);
    }

    private bool _disposed;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Clear cache on disposal
                _cachedTasks = null;
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
