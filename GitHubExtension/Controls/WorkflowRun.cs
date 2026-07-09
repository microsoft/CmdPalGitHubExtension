// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.Controls;

public sealed class WorkflowRun : IWorkflowRun
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Conclusion { get; set; } = string.Empty;

    public string HeadBranch { get; set; } = string.Empty;

    public string TriggerEvent { get; set; } = string.Empty;

    public long RunNumber { get; set; }

    public string HtmlUrl { get; set; } = string.Empty;

    public string RepositoryFullName { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
