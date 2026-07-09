// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Serilog;

namespace GitHubExtension.Controls.Commands;

// Runs a single write action against GitHub, showing a success or error toast
// and notifying the mutation mediator so open surfaces can refresh. Non
// destructive actions use this command directly; destructive actions wrap it
// in a ConfirmedCommand.
internal sealed partial class GitHubMutationCommand : InvokableCommand
{
    private static readonly ILogger _log = Log.ForContext("SourceContext", nameof(GitHubMutationCommand));

    private readonly Func<Task> _mutation;
    private readonly string _successMessage;
    private readonly string _errorMessage;
    private readonly MutationMediator _mediator;

    internal GitHubMutationCommand(string name, IconInfo icon, Func<Task> mutation, string successMessage, string errorMessage, MutationMediator mediator)
    {
        Name = name;
        Icon = icon;
        _mutation = mutation;
        _successMessage = successMessage;
        _errorMessage = errorMessage;
        _mediator = mediator;
    }

    public override CommandResult Invoke()
    {
        try
        {
            _mutation().GetAwaiter().GetResult();
            ToastHelper.ShowSuccessToast(_successMessage);
            _mediator.NotifyMutationCompleted();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "GitHub write action failed.");
            ToastHelper.ShowErrorToast(_errorMessage);
        }

        return CommandResult.KeepOpen();
    }
}
