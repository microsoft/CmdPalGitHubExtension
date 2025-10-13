// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using GitHubExtension.Helpers;
using GitHubExtension.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Forms;

public partial class GitHubCopilotForm : FormContent, IDisposable
{
    private readonly IGitHubCopilotService _copilotService;
    private readonly IResources _resources;
    private string _lastResponse = string.Empty;
    private string _lastPrompt = string.Empty;
    private string _lastRepository = string.Empty;
    private string _lastBaseBranch = string.Empty;
    private bool _isProcessing;

    public GitHubCopilotForm(IGitHubCopilotService copilotService, IResources resources)
    {
        _copilotService = copilotService;
        _resources = resources;
    }

    public Dictionary<string, string> TemplateSubstitutions => new()
    {
        { "{{CopilotIcon}}", JsonSerializer.Serialize($"data:image/png;base64,{GitHubIcon.GetBase64Icon(GitHubIcon.LogoWithBackplatePath)}") },
        { "{{CopilotTitle}}", JsonSerializer.Serialize(_resources.GetResource("Forms_GitHubCopilot_Title")) },
        { "{{CopilotDescription}}", JsonSerializer.Serialize(_resources.GetResource("Forms_GitHubCopilot_Description")) },
        { "{{PromptLabel}}", JsonSerializer.Serialize(_resources.GetResource("Forms_GitHubCopilot_PromptLabel")) },
        { "{{PromptPlaceholder}}", JsonSerializer.Serialize(_resources.GetResource("Forms_GitHubCopilot_PromptPlaceholder")) },
        { "{{PromptErrorMessage}}", JsonSerializer.Serialize(_resources.GetResource("Forms_GitHubCopilot_PromptError")) },
        { "{{RepositoryLabel}}", JsonSerializer.Serialize(_resources.GetResource("Forms_GitHubCopilot_RepositoryLabel")) },
        { "{{RepositoryPlaceholder}}", JsonSerializer.Serialize(_resources.GetResource("Forms_GitHubCopilot_RepositoryPlaceholder")) },
        { "{{RepositoryErrorMessage}}", JsonSerializer.Serialize(_resources.GetResource("Forms_GitHubCopilot_RepositoryError")) },
        { "{{BaseBranchLabel}}", JsonSerializer.Serialize(_resources.GetResource("Forms_GitHubCopilot_BaseBranchLabel")) },
        { "{{BaseBranchPlaceholder}}", JsonSerializer.Serialize(_resources.GetResource("Forms_GitHubCopilot_BaseBranchPlaceholder")) },
        { "{{ResponseLabel}}", JsonSerializer.Serialize(_resources.GetResource("Forms_GitHubCopilot_ResponseLabel")) },
        { "{{ResponseText}}", JsonSerializer.Serialize(_lastResponse) },
        { "{{SubmitButtonTitle}}", JsonSerializer.Serialize(_resources.GetResource("Forms_GitHubCopilot_SubmitButton")) },
        { "{{ClearButtonTitle}}", JsonSerializer.Serialize(_resources.GetResource("Forms_GitHubCopilot_ClearButton")) },
        { "{{HasResponse}}", JsonSerializer.Serialize(!string.IsNullOrEmpty(_lastResponse)) },
        { "{{PromptValue}}", JsonSerializer.Serialize(_lastPrompt) },
        { "{{RepositoryValue}}", JsonSerializer.Serialize(_lastRepository) },
        { "{{BaseBranchValue}}", JsonSerializer.Serialize(_lastBaseBranch) },
    };

    public override string TemplateJson => TemplateHelper.LoadTemplateJsonFromTemplateName("GitHubCopilotTemplate", TemplateSubstitutions);

    public override ICommandResult SubmitForm(string inputs, string data)
    {
        if (_isProcessing)
        {
            return CommandResult.KeepOpen();
        }

        try
        {
            var inputData = JsonSerializer.Deserialize<Dictionary<string, object>>(inputs);
            var actionData = JsonSerializer.Deserialize<Dictionary<string, object>>(data);

            if (actionData?.TryGetValue("id", out var actionId) == true)
            {
                var actionIdString = actionId.ToString();

                if (actionIdString == "ClearResponse")
                {
                    ClearResponse();
                    return CommandResult.KeepOpen();
                }

                if (actionIdString == "SubmitRequest")
                {
                    var prompt = inputData?.TryGetValue("Prompt", out var promptObj) == true ? promptObj.ToString()?.Trim() : string.Empty;
                    var repository = inputData?.TryGetValue("Repository", out var repoObj) == true ? repoObj.ToString()?.Trim() : string.Empty;
                    var baseBranch = inputData?.TryGetValue("BaseBranch", out var branchObj) == true ? branchObj.ToString()?.Trim() : string.Empty;

                    if (!string.IsNullOrEmpty(prompt))
                    {
                        _lastPrompt = prompt;
                        _lastRepository = repository ?? string.Empty;
                        _lastBaseBranch = baseBranch ?? string.Empty;

                        // Process the request asynchronously
                        Task.Run(async () => await ProcessCopilotRequest(prompt, repository ?? string.Empty, baseBranch ?? string.Empty));
                        return CommandResult.KeepOpen();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _lastResponse = $"Error processing request: {ex.Message}";
            RefreshTemplate();
        }

        return CommandResult.KeepOpen();
    }

    private async Task ProcessCopilotRequest(string prompt, string repository, string baseBranch)
    {
        _isProcessing = true;

        try
        {
            // Show loading state
            _lastResponse = _resources.GetResource("Forms_GitHubCopilot_Processing");
            RefreshTemplate();

            // Build context-aware message
            var contextualPrompt = BuildContextualPrompt(prompt, repository, baseBranch);

            // Get response from Copilot service
            var response = await _copilotService.SendMessageAsync(contextualPrompt);
            _lastResponse = response;
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error: {ex.Message}";
            _lastResponse = errorMessage;
        }
        finally
        {
            _isProcessing = false;
            RefreshTemplate();
        }
    }

    private string BuildContextualPrompt(string prompt, string repository, string baseBranch)
    {
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

        return contextualPrompt;
    }

    private void ClearResponse()
    {
        _lastResponse = string.Empty;
        _lastPrompt = string.Empty;
        _lastRepository = string.Empty;
        _lastBaseBranch = string.Empty;
        RefreshTemplate();
    }

    private void RefreshTemplate()
    {
        OnPropertyChanged(nameof(TemplateJson));
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
#pragma warning restore SA1518
