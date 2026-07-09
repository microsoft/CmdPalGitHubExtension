// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;

namespace GitHubExtension.DataManager;

public interface IWorkflowRunsDataManager
{
    // Fetches the most recent workflow runs for the given repository. The
    // repository may be provided as the short owner/repo form or a full URL.
    Task<IEnumerable<IWorkflowRun>> GetWorkflowRunsAsync(string ownerRepo);

    // Re-runs all jobs for a workflow run. This is a non-destructive action.
    Task RerunAsync(string ownerRepo, long runId);

    // Cancels an in-progress workflow run. This is a destructive action.
    Task CancelAsync(string ownerRepo, long runId);

    // Triggers a workflow_dispatch event for the given workflow file on the
    // provided git reference (branch or tag).
    Task TriggerWorkflowAsync(string ownerRepo, string workflowFileName, string gitRef);
}
