// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using GitHubExtension.Controls.Commands;
using GitHubExtension.Helpers;
using GitHubExtension.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Forms;

public partial class CopilotChatForm : FormContent, IDisposable
{
    private readonly IGitHubCopilotService _copilotService;
    private readonly IResources _resources;
    private string _lastResponse = string.Empty;
    private string _lastQuestion = string.Empty;
    private bool _isProcessing;

    public CopilotChatForm(IGitHubCopilotService copilotService, IResources resources)
    {
        _copilotService = copilotService;
        _resources = resources;
    }

    public Dictionary<string, string> TemplateSubstitutions => new()
    {
        { "{{CopilotIcon}}", JsonSerializer.Serialize($"data:image/png;base64,{GitHubIcon.GetBase64Icon(GitHubIcon.LogoWithBackplatePath)}") },
        { "{{CopilotTitle}}", JsonSerializer.Serialize(_resources.GetResource("Forms_Copilot_Title")) },
        { "{{CopilotDescription}}", JsonSerializer.Serialize("Ask GitHub Copilot for help with your development questions. Your answer will appear both here and on the main GitHub MCP page.") },
        { "{{InputLabel}}", JsonSerializer.Serialize(_resources.GetResource("Forms_Copilot_InputLabel")) },
        { "{{InputPlaceholder}}", JsonSerializer.Serialize(_resources.GetResource("Forms_Copilot_InputPlaceholder")) },
        { "{{InputErrorMessage}}", JsonSerializer.Serialize(_resources.GetResource("Forms_Copilot_InputError")) },
        { "{{ResponseLabel}}", JsonSerializer.Serialize(_resources.GetResource("Forms_Copilot_ResponseLabel")) },
        { "{{ResponseText}}", JsonSerializer.Serialize(_lastResponse) },
        { "{{SendButtonTitle}}", JsonSerializer.Serialize(_resources.GetResource("Forms_Copilot_SendButton")) },
        { "{{ClearButtonTitle}}", JsonSerializer.Serialize(_resources.GetResource("Forms_Copilot_ClearButton")) },
        { "{{HasResponse}}", JsonSerializer.Serialize(!string.IsNullOrEmpty(_lastResponse)) },
    };

    public override string TemplateJson => TemplateHelper.LoadTemplateJsonFromTemplateName("CopilotChatTemplate", TemplateSubstitutions);

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

                if (actionIdString == "SendMessage")
                {
                    if (inputData?.TryGetValue("UserMessage", out var messageObj) == true)
                    {
                        var message = messageObj.ToString()?.Trim();
                        if (!string.IsNullOrEmpty(message))
                        {
                            _lastQuestion = message;

                            // Process the message asynchronously
                            Task.Run(async () => await ProcessUserMessage(message));
                            return CommandResult.KeepOpen();
                        }
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

    private async Task ProcessUserMessage(string message)
    {
        _isProcessing = true;

        try
        {
            // Show loading state
            _lastResponse = _resources.GetResource("Forms_Copilot_Processing");
            RefreshTemplate();

            // Get response from Copilot service
            var response = await _copilotService.SendMessageAsync(message);
            _lastResponse = response;

            // Store the response for the MCP page to display
            CopilotResponseStorage.StoreResponse(message, response);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error: {ex.Message}";
            _lastResponse = errorMessage;

            // Store the error response as well
            CopilotResponseStorage.StoreResponse(message, errorMessage);
        }
        finally
        {
            _isProcessing = false;
            RefreshTemplate();
        }
    }

    private void ClearResponse()
    {
        _lastResponse = string.Empty;
        _lastQuestion = string.Empty;
        CopilotResponseStorage.ClearResponse();
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
