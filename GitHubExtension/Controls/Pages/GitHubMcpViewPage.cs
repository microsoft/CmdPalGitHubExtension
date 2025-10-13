// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Helpers;
using GitHubExtension.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Pages;

public sealed partial class GitHubMcpViewPage : ListPage, IDisposable
{
    private readonly IResources _resources;
    private readonly IGitHubCopilotService _copilotService;

    public GitHubMcpViewPage(IResources resources, IGitHubCopilotService copilotService)
    {
        _resources = resources;
        _copilotService = copilotService;
    }

    public override IListItem[] GetItems()
    {
        var items = new List<IListItem>();

        try
        {
            // Get Copilot tasks asynchronously
            var tasks = _copilotService.GetCopilotTasksAsync().GetAwaiter().GetResult();

            foreach (var task in tasks)
            {
                var taskUrl = task.Url ?? string.Empty;
                var openUrlCommand = new OpenUrlCommand(taskUrl);
                var listItem = new ListItem(openUrlCommand)
                {
                    Title = task.Title,
                    Subtitle = $"{task.Repository} • {task.Status} • {task.Agent}",
                    Icon = GitHubIcon.IconDictionary["github"],
                };

                items.Add(listItem);
            }

            if (items.Count == 0)
            {
                var noTasksItem = new ListItem(new NoOpCommand())
                {
                    Title = "No Copilot tasks found",
                    Subtitle = "Create a new task to get started with GitHub Copilot Workspace",
                    Icon = GitHubIcon.IconDictionary["github"],
                };
                items.Add(noTasksItem);
            }
        }
        catch (Exception ex)
        {
            var errorItem = new ListItem(new NoOpCommand())
            {
                Title = "Error loading tasks",
                Subtitle = ex.Message,
                Icon = GitHubIcon.IconDictionary["github"],
            };
            items.Add(errorItem);
        }

        return items.ToArray();
    }

    // Disposing area
    private bool _disposed;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose any managed resources if needed
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
