// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.Controls;

public class WorkflowRunsMediator
{
    public event EventHandler<object?>? WorkflowRunsChanged;

    public WorkflowRunsMediator()
    {
    }

    public void NotifyWorkflowRunsChanged(object? args = null)
    {
        WorkflowRunsChanged?.Invoke(this, args);
    }
}
