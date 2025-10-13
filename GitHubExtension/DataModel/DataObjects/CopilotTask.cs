// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Serialization;

namespace GitHubExtension.DataModel.DataObjects;

/// <summary>
/// Represents a GitHub Copilot coding agent task.
/// </summary>
public class CopilotTask
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public CopilotTaskStatus Status { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("repository")]
    public string? Repository { get; set; }

    [JsonPropertyName("branch")]
    public string? Branch { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("agent")]
    public string Agent { get; set; } = "GitHub Copilot";

    public override string ToString() => $"{Title} ({Status})";
}

/// <summary>
/// Represents the status of a Copilot task.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CopilotTaskStatus
{
    [JsonPropertyName("in_progress")]
    InProgress,

    [JsonPropertyName("completed")]
    Completed,

    [JsonPropertyName("failed")]
    Failed,

    [JsonPropertyName("cancelled")]
    Cancelled,
}
