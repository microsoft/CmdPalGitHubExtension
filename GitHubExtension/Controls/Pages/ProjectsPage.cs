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

// Lists the signed-in user's Projects (v2). Open a project in the browser or
// copy its URL.
public sealed partial class ProjectsPage : ListPage
{
    private readonly IProjectsDataManager _dataManager;
    private readonly IResources _resources;
    private readonly ILogger _log;

    public ProjectsPage(IProjectsDataManager dataManager, IResources resources)
    {
        _dataManager = dataManager;
        _resources = resources;
        _log = Log.ForContext("SourceContext", $"Pages/{nameof(ProjectsPage)}");

        Name = resources.GetResource("Pages_Projects");
        Icon = GitHubIcon.IconDictionary["Projects"];
    }

    public override IListItem[] GetItems() => DoGetItems().GetAwaiter().GetResult();

    private async Task<IListItem[]> DoGetItems()
    {
        try
        {
            var projects = (await _dataManager.GetMyProjectsAsync()).ToList();

            if (projects.Count == 0)
            {
                return new IListItem[]
                {
                    new ListItem(new NoOpCommand())
                    {
                        Title = _resources.GetResource("Pages_Projects_None"),
                        Icon = GitHubIcon.IconDictionary["Projects"],
                    },
                };
            }

            return projects.Select(GetListItem).ToArray();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to load projects.");
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

    private ListItem GetListItem(IProject project)
    {
        var stateKey = project.Closed ? "Pages_Projects_Closed" : "Pages_Projects_Open";

        return new ListItem(new LinkCommand(project.HtmlUrl, _resources))
        {
            Title = project.Title,
            Subtitle = _resources.GetResource(stateKey),
            Icon = GitHubIcon.IconDictionary["Projects"],
            MoreCommands = new CommandContextItem[]
            {
                new(new CopyCommand(project.HtmlUrl, _resources.GetResource("Commands_CopyURL"), _resources)),
            },
        };
    }
}
