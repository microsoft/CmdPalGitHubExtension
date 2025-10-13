// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma warning disable SA1518 // File is required to end with a single newline character

namespace GitHubExtension.Controls.Commands;

/// <summary>
/// Helper class to store Copilot responses for the MCP page.
/// </summary>
public static class CopilotResponseStorage
{
    private static string? _lastQuestion;
    private static string? _lastResponse;
    private static DateTime _lastResponseTime;

    public static void StoreResponse(string question, string response)
    {
        _lastQuestion = question;
        _lastResponse = response;
        _lastResponseTime = DateTime.Now;
    }

    public static (string? Question, string? Response, DateTime ResponseTime) GetLastResponse()
    {
        return (_lastQuestion, _lastResponse, _lastResponseTime);
    }

    public static void ClearResponse()
    {
        _lastQuestion = null;
        _lastResponse = null;
        _lastResponseTime = default;
    }
}