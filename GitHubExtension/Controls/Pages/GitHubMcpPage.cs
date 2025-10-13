// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls.Forms;
using GitHubExtension.DeveloperIds;
using GitHubExtension.Helpers;
using GitHubExtension.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Pages;

public sealed partial class GitHubMcpPage : ListPage, IDisposable
{
    private readonly IResources _resources;
    private readonly IDeveloperIdProvider _developerIdProvider;
    private readonly GitHubMcpFormPage _createTaskFormPage;
    private readonly GitHubMcpViewPage _viewTasksPage;

    public GitHubMcpPage(IResources resources, IDeveloperIdProvider developerIdProvider, IGitHubCopilotService copilotService)
    {
        _resources = resources;
        _developerIdProvider = developerIdProvider;

        // Create the form for creating tasks
        var createTaskForm = new GitHubMcpForm(developerIdProvider, resources);
        _createTaskFormPage = new GitHubMcpFormPage(createTaskForm, new StatusMessage(), resources);

        // Create the view tasks page
        _viewTasksPage = new GitHubMcpViewPage(resources, copilotService);
    }

    public override IListItem[] GetItems()
    {
        var items = new List<IListItem>();

        // Add "Create Task" option
        var createTaskItem = new ListItem(_createTaskFormPage)
        {
            Title = _resources.GetResource("Commands_GitHub_Copilot_CreateTask"),
            Subtitle = _resources.GetResource("Pages_GitHub_Copilot_CreateTask_Description"),
            Icon = GitHubIcon.IconDictionary["github"],
        };
        items.Add(createTaskItem);

        // Add "View Tasks" option
        var viewTasksItem = new ListItem(_viewTasksPage)
        {
            Title = _resources.GetResource("Commands_GitHub_Copilot_ViewTask"),
            Subtitle = _resources.GetResource("Pages_GitHub_Copilot_ViewTask_Description"),
            Icon = GitHubIcon.IconDictionary["github"],
        };
        items.Add(viewTasksItem);

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
                _createTaskFormPage?.Dispose();
                _viewTasksPage?.Dispose();
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
