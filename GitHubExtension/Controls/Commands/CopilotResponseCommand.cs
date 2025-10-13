// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma warning disable SA1518 // File is required to end with a single newline character

using GitHubExtension.Helpers;
using GitHubExtension.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Serilog;

namespace GitHubExtension.Controls.Commands;

public sealed partial class CopilotResponseCommand : InvokableCommand, IDisposable
{
    private readonly string _question;
    private readonly IGitHubCopilotService _copilotService;
    private readonly IResources _resources;
    private readonly ILogger _logger;
    private string? _cachedResponse;
    private bool _isProcessing;

    public CopilotResponseCommand(string question, IGitHubCopilotService copilotService, IResources resources)
    {
        _question = question;
        _copilotService = copilotService;
        _resources = resources;
        _logger = Log.ForContext("SourceContext", nameof(CopilotResponseCommand));

        Name = $"Ask GitHub Copilot: \"{question.Substring(0, Math.Min(question.Length, 50))}{(question.Length > 50 ? "..." : string.Empty)}\"";
        Icon = GitHubIcon.IconDictionary["logo"];
    }

    public override CommandResult Invoke()
    {
        if (_isProcessing)
        {
            return CommandResult.KeepOpen();
        }

        if (!string.IsNullOrEmpty(_cachedResponse))
        {
            // Store the response and show status
            CopilotResponseStorage.StoreResponse(_question, _cachedResponse);
            ShowResponse(_cachedResponse);
            return CommandResult.KeepOpen();
        }

        // Process the question asynchronously and show processing message
        _isProcessing = true;
        ShowProcessingMessage();

        Task.Run(async () => await ProcessQuestionAsync());

        return CommandResult.KeepOpen();
    }

    private async Task ProcessQuestionAsync()
    {
        try
        {
            _logger.Information($"Processing Copilot question: {_question}");
            var response = await _copilotService.SendMessageAsync(_question);
            _cachedResponse = response;

            // Store the response for the MCP page
            CopilotResponseStorage.StoreResponse(_question, response);

            // Update the command name to indicate completion
            Name = $"View GitHub Copilot Response: \"{_question.Substring(0, Math.Min(_question.Length, 40))}{(_question.Length > 40 ? "..." : string.Empty)}\"";

            ShowCompletionMessage(response);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error processing Copilot question");
            var errorMessage = $"Error: {ex.Message}";
            _cachedResponse = errorMessage;

            // Store the error response
            CopilotResponseStorage.StoreResponse(_question, errorMessage);

            Name = $"GitHub Copilot Error: \"{_question.Substring(0, Math.Min(_question.Length, 30))}{(_question.Length > 30 ? "..." : string.Empty)}\"";

            ShowCompletionMessage(errorMessage);
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private void ShowProcessingMessage()
    {
        var statusMessage = new StatusMessage
        {
            Message = _resources.GetResource("Forms_Copilot_Processing"),
            State = MessageState.Info,
        };

        ExtensionHost.ShowStatus(statusMessage, StatusContext.Page);
    }

    private void ShowCompletionMessage(string response)
    {
        var statusMessage = new StatusMessage
        {
            Message = "GitHub Copilot response ready. Navigate to the 'GitHub MCP' page to view the full response, or click this command again.",
            State = MessageState.Success,
        };

        ExtensionHost.ShowStatus(statusMessage, StatusContext.Page);
    }

    private void ShowResponse(string response)
    {
        var statusMessage = new StatusMessage
        {
            Message = $"GitHub Copilot: {response.Substring(0, Math.Min(response.Length, 200))}{(response.Length > 200 ? "... (Navigate to GitHub MCP page for full response)" : string.Empty)}",
            State = MessageState.Success,
        };

        ExtensionHost.ShowStatus(statusMessage, StatusContext.Page);
    }

    private bool _disposed;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // No managed resources to dispose
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