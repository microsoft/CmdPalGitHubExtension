// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Controls.Commands;
using GitHubExtension.DataModel.DataObjects;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Pages;

/// <summary>
/// Detail page for individual Copilot tasks.
/// </summary>
public sealed class CopilotTaskDetailPage : ContentPage
{
    private readonly IResources _resources;
    private readonly CopilotTask _task;

    public CopilotTaskDetailPage(IResources resources, CopilotTask task)
    {
        _resources = resources;
        _task = task;

        Icon = GitHubIcon.IconDictionary["logo"];
        Name = $"Task: {task.Title}";

        // Build command list dynamically based on available data
        var commands = new List<CommandContextItem>
        {
            new CommandContextItem(new OpenTaskUrlCommand(task.Url, resources))
            {
                Title = "Open in Browser",
                Icon = new IconInfo("\uE774"), // Globe icon
            },
            new CommandContextItem(new CopyCommand(task.Url ?? string.Empty, _resources.GetResource("Commands_CopyURL"), _resources))
            {
                Title = _resources.GetResource("Commands_CopyURL"),
                Icon = new IconInfo("\uE8C8"), // Copy icon
            },
            new CommandContextItem(new CopyCommand(task.Title, "Copy Task Title", _resources))
            {
                Title = "Copy Task Title",
                Icon = new IconInfo("\uE8C8"), // Copy icon
            },
        };

        // Add branch copy command if branch is available
        if (!string.IsNullOrEmpty(task.Branch))
        {
            commands.Add(new CommandContextItem(new CopyCommand(task.Branch, "Copy Branch Name", _resources))
            {
                Title = "Copy Branch Name",
                Icon = new IconInfo("\uE8C8"), // Copy icon
            });
        }

        // Add repository copy command if repository is available
        if (!string.IsNullOrEmpty(task.Repository))
        {
            commands.Add(new CommandContextItem(new CopyCommand(task.Repository, "Copy Repository Name", _resources))
            {
                Title = "Copy Repository Name",
                Icon = new IconInfo("\uE8C8"), // Copy icon
            });
        }

        Commands = commands.ToArray();
    }

    public override IContent[] GetContent()
    {
        var statusText = GetStatusText(_task.Status);
        var statusEmoji = GetStatusEmoji(_task.Status);
        var timeAgo = GetRelativeTime(_task.UpdatedAt);

        var content = new MarkdownContent
        {
            Body = $"""
                # {_task.Title}

                ## Status: {statusEmoji} {statusText}

                **Authored by:** {_task.Agent}  
                **Last Updated:** {timeAgo}

                ---

                ## Description

                {_task.Description}

                ---

                ## Pull Request Details

                | Property | Value |
                |----------|-------|
                | **ID** | `{_task.Id}` |
                | **Status** | {statusEmoji} {statusText} |
                | **Repository** | {_task.Repository ?? "Not specified"} |
                | **Branch** | {_task.Branch ?? "Not specified"} |
                | **Agent** | {_task.Agent} |
                | **Created** | {_task.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC |
                | **Updated** | {_task.UpdatedAt:yyyy-MM-dd HH:mm:ss} UTC |

                {GetPullRequestInfo(_task)}

                ---

                {GetActionableSteps(_task)}

                ---

                *Use the commands above to open in browser, copy details, or perform other actions.*
                """,
        };

        return new IContent[] { content };
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

    private static string GetStatusEmoji(CopilotTaskStatus status)
    {
        return status switch
        {
            CopilotTaskStatus.Completed => "?",
            CopilotTaskStatus.InProgress => "??",
            CopilotTaskStatus.Failed => "?",
            CopilotTaskStatus.Cancelled => "??",
            _ => "?",
        };
    }

    private static string GetRelativeTime(DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;

        if (timeSpan.TotalMinutes < 1)
        {
            return "Just now";
        }

        if (timeSpan.TotalMinutes < 60)
        {
            var minutes = (int)timeSpan.TotalMinutes;
            return $"{minutes} {(minutes == 1 ? "minute" : "minutes")} ago";
        }

        if (timeSpan.TotalHours < 24)
        {
            var hours = (int)timeSpan.TotalHours;
            return $"{hours} {(hours == 1 ? "hour" : "hours")} ago";
        }

        if (timeSpan.TotalDays < 7)
        {
            var days = (int)timeSpan.TotalDays;
            return $"{days} {(days == 1 ? "day" : "days")} ago";
        }

        if (timeSpan.TotalDays < 30)
        {
            var weeks = (int)(timeSpan.TotalDays / 7);
            return $"{weeks} {(weeks == 1 ? "week" : "weeks")} ago";
        }

        return dateTime.ToString("MMM dd, yyyy", CultureInfo.InvariantCulture);
    }

    private static string GetPullRequestInfo(CopilotTask task)
    {
        if (task.Status == CopilotTaskStatus.InProgress)
        {
            return """
                ## ?? Open Pull Request

                This pull request was created by GitHub Copilot and is currently open for review.

                **What This Means:**
                - The changes have been proposed but not yet merged
                - The code is ready for your review and feedback
                - CI/CD checks may be running or completed
                - Other team members may have already left comments

                **Next Steps:**
                1. Click "Open in Browser" above to review the changes on GitHub
                2. Examine the code diff and proposed changes
                3. Test the changes locally if needed (use git checkout command)
                4. Leave inline comments or request changes
                5. Approve and merge when satisfied with the quality
                """;
        }

        if (task.Status == CopilotTaskStatus.Completed)
        {
            return """
                ## ? Completed Pull Request

                This pull request from GitHub Copilot has been successfully completed and merged.
                
                **Status:**
                - ? Changes have been integrated into the target branch
                - ? Pull request is closed and merged
                - ? Code is now part of the repository
                
                **Actions Available:**
                - View the merged changes and discussion history
                - See who reviewed and approved the changes
                - Check the final commit that was merged
                
                Visit the URL above to see the complete history and outcome.
                """;
        }

        if (task.Status == CopilotTaskStatus.Failed)
        {
            return """
                ## ? Pull Request Issues

                This pull request has encountered problems and requires attention.
                
                **Common Causes:**
                - ?? **Merge Conflicts** - Changes conflict with the target branch
                - ?? **Failed CI/CD Checks** - Automated tests or builds failed
                - ?? **Review Concerns** - Reviewers found issues requiring fixes
                - ?? **Code Quality Issues** - Linting or static analysis failures
                
                **How to Resolve:**
                1. Open the PR in your browser to see specific error details
                2. Review failed checks and error messages
                3. Address any merge conflicts by updating the branch
                4. Fix code issues identified by reviewers or automated checks
                5. Push new commits to update the PR
                
                Visit the URL above to diagnose and fix the specific issues.
                """;
        }

        if (task.Status == CopilotTaskStatus.Cancelled)
        {
            return """
                ## ?? Pull Request Closed

                This pull request was closed without being merged into the target branch.
                
                **Possible Reasons:**
                - ?? **Superseded** - A better or alternative solution was implemented
                - ?? **Not Needed** - Requirements changed or feature was no longer needed  
                - ?? **Review Rejected** - Changes didn't meet code quality or project standards
                - ?? **Stale** - PR became outdated or was abandoned
                
                **Note:** Even though this PR wasn't merged, you can:
                - View the proposed changes for future reference
                - Reopen the PR if circumstances change
                - Learn from the discussion and review comments
                - Use parts of the code in future implementations
                
                You can still view all proposed changes by visiting the URL above.
                """;
        }

        return string.Empty;
    }

    private static string GetActionableSteps(CopilotTask task)
    {
        if (task.Status == CopilotTaskStatus.InProgress && !string.IsNullOrEmpty(task.Branch))
        {
            return $"""
                ## ??? Quick Actions

                **Review the Changes Locally:**
                ```bash
                git fetch origin
                git checkout {task.Branch}
                ```

                **Test the Changes:**
                ```bash
                # Run your project's test suite
                # Build and test the application locally
                ```

                **Useful Git Commands:**
                ```bash
                # View the changes
                git log origin/main..{task.Branch}
                
                # See the diff
                git diff origin/main...{task.Branch}
                
                # Return to your previous branch
                git checkout -
                ```
                """;
        }

        return string.Empty;
    }
}
