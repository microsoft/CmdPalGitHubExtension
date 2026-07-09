// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataManager;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Serilog;

namespace GitHubExtension.Controls.Commands;

// Runs a workflow-run action (re-run or cancel) against GitHub, showing a
// success or error toast and notifying the workflow-runs mediator so the open
// page can refresh. Non-destructive actions use this command directly;
// destructive actions wrap it in a ConfirmedCommand.
internal sealed partial class WorkflowRunActionCommand : InvokableCommand
{
    private static readonly ILogger _log = Log.ForContext("SourceContext", nameof(WorkflowRunActionCommand));

    private readonly Func<Task> _action;
    private readonly string _successMessage;
    private readonly string _errorMessage;
    private readonly WorkflowRunsMediator _mediator;

    internal WorkflowRunActionCommand(string name, IconInfo icon, Func<Task> action, string successMessage, string errorMessage, WorkflowRunsMediator mediator)
    {
        Name = name;
        Icon = icon;
        _action = action;
        _successMessage = successMessage;
        _errorMessage = errorMessage;
        _mediator = mediator;
    }

    public override CommandResult Invoke()
    {
        try
        {
            _action().GetAwaiter().GetResult();
            ToastHelper.ShowSuccessToast(_successMessage);
            _mediator.NotifyWorkflowRunsChanged();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Workflow run action failed.");
            ToastHelper.ShowErrorToast(_errorMessage);
        }

        return CommandResult.KeepOpen();
    }
}
