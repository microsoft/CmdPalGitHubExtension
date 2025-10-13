// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls.Pages;
using GitHubExtension.Helpers;
using GitHubExtension.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Serilog;

namespace GitHubExtension.Controls.Pages;

public sealed partial class GitHubCopilotPage : ListPage, IDisposable
{
    private readonly IResources _resources;
    private readonly IGitHubCopilotService _copilotService;
    private readonly ILogger _logger;
    private readonly GitHubCopilotFormPage _copilotFormPage;
    private readonly GitHubCopilotViewPage _copilotViewPage;

    public GitHubCopilotPage(IResources resources, IGitHubCopilotService copilotService)
    {
        _resources = resources;
        _copilotService = copilotService;
        _logger = Log.ForContext("SourceContext", nameof(GitHubCopilotPage));

        Icon = GitHubIcon.IconDictionary["logo"];
        Name = _resources.GetResource("Pages_GitHubCopilot_Title");

        // Create both subpages, passing the copilot service to the view page
        _copilotViewPage = new GitHubCopilotViewPage(_resources, _copilotService);
        _copilotFormPage = new GitHubCopilotFormPage(_resources, _copilotService);
    }

    public override IListItem[] GetItems()
    {
        var viewTaskItem = new ListItem(_copilotViewPage)
        {
            Title = _resources.GetResource("Commands_GitHub_Copilot_ViewTask"),
            Subtitle = _resources.GetResource("Pages_GitHub_Copilot_ViewTask_Description"),
            Icon = GitHubIcon.IconDictionary["logo"],
        };

        var createTaskItem = new ListItem(_copilotFormPage)
        {
            Title = _resources.GetResource("Commands_GitHub_Copilot_CreateTask"),
            Subtitle = _resources.GetResource("Pages_GitHub_Copilot_CreateTask_Description"),
            Icon = GitHubIcon.IconDictionary["logo"],
        };

        return [viewTaskItem, createTaskItem];
    }

    public async Task<string> ProcessCopilotRequestAsync(string prompt, string repository = "", string baseBranch = "")
    {
        try
        {
            _logger.Information($"Processing Copilot request: {prompt} | Repo: {repository} | Branch: {baseBranch}");

            // Build contextual prompt
            var contextualPrompt = prompt;
            if (!string.IsNullOrEmpty(repository) || !string.IsNullOrEmpty(baseBranch))
            {
                contextualPrompt += "\n\nContext:";
                if (!string.IsNullOrEmpty(repository))
                {
                    contextualPrompt += $"\n- Repository: {repository}";
                }

                if (!string.IsNullOrEmpty(baseBranch))
                {
                    contextualPrompt += $"\n- Base Branch: {baseBranch}";
                }
            }

            var response = await _copilotService.SendMessageAsync(contextualPrompt);
            return response;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error processing Copilot request");
            return $"Error: {ex.Message}";
        }
    }

    private bool _disposed;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _copilotFormPage?.Dispose();
                _copilotViewPage?.Dispose();
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
#pragma warning restore SA1518
