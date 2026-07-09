// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataManager;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Commands;

// Builds the set of write-action context items for an issue or pull request.
// The available actions depend on the item's current state (open vs closed).
public sealed class MutationCommandsFactory
{
    private readonly IGitHubMutationManager _mutationManager;
    private readonly MutationMediator _mediator;
    private readonly IResources _resources;

    private static readonly IconInfo CloseIcon = new("\uE711");
    private static readonly IconInfo ReopenIcon = new("\uE72C");
    private static readonly IconInfo MergeIcon = new("\uE8FB");

    public MutationCommandsFactory(IGitHubMutationManager mutationManager, MutationMediator mediator, IResources resources)
    {
        _mutationManager = mutationManager;
        _mediator = mediator;
        _resources = resources;
    }

    private static bool IsOpen(IIssue issue) => string.Equals(issue.State, "Open", StringComparison.OrdinalIgnoreCase);

    public IEnumerable<CommandContextItem> GetIssueCommands(IIssue issue)
    {
        var items = new List<CommandContextItem>();

        if (IsOpen(issue))
        {
            var close = new GitHubMutationCommand(
                _resources.GetResource("Commands_CloseIssue"),
                CloseIcon,
                () => _mutationManager.CloseIssueAsync(issue),
                _resources.GetResource("Message_CloseIssue_Success"),
                _resources.GetResource("Message_CloseIssue_Error"),
                _mediator);

            var confirmed = new ConfirmedCommand(
                close,
                _resources.GetResource("Commands_CloseIssue"),
                CloseIcon,
                _resources.GetResource("Confirm_CloseIssue_Title"),
                _resources.GetResource("Confirm_CloseIssue_Description"));

            items.Add(new CommandContextItem(confirmed) { IsCritical = true });
        }
        else
        {
            var reopen = new GitHubMutationCommand(
                _resources.GetResource("Commands_ReopenIssue"),
                ReopenIcon,
                () => _mutationManager.ReopenIssueAsync(issue),
                _resources.GetResource("Message_ReopenIssue_Success"),
                _resources.GetResource("Message_ReopenIssue_Error"),
                _mediator);

            items.Add(new CommandContextItem(reopen));
        }

        return items;
    }

    public IEnumerable<CommandContextItem> GetPullRequestCommands(IPullRequest pullRequest)
    {
        var items = new List<CommandContextItem>();

        if (IsOpen(pullRequest))
        {
            var merge = new GitHubMutationCommand(
                _resources.GetResource("Commands_MergePullRequest"),
                MergeIcon,
                () => _mutationManager.MergePullRequestAsync(pullRequest),
                _resources.GetResource("Message_MergePullRequest_Success"),
                _resources.GetResource("Message_MergePullRequest_Error"),
                _mediator);

            var confirmedMerge = new ConfirmedCommand(
                merge,
                _resources.GetResource("Commands_MergePullRequest"),
                MergeIcon,
                _resources.GetResource("Confirm_MergePullRequest_Title"),
                _resources.GetResource("Confirm_MergePullRequest_Description"));

            items.Add(new CommandContextItem(confirmedMerge) { IsCritical = true });

            var close = new GitHubMutationCommand(
                _resources.GetResource("Commands_ClosePullRequest"),
                CloseIcon,
                () => _mutationManager.ClosePullRequestAsync(pullRequest),
                _resources.GetResource("Message_ClosePullRequest_Success"),
                _resources.GetResource("Message_ClosePullRequest_Error"),
                _mediator);

            var confirmedClose = new ConfirmedCommand(
                close,
                _resources.GetResource("Commands_ClosePullRequest"),
                CloseIcon,
                _resources.GetResource("Confirm_ClosePullRequest_Title"),
                _resources.GetResource("Confirm_ClosePullRequest_Description"));

            items.Add(new CommandContextItem(confirmedClose) { IsCritical = true });
        }
        else
        {
            var reopen = new GitHubMutationCommand(
                _resources.GetResource("Commands_ReopenPullRequest"),
                ReopenIcon,
                () => _mutationManager.ReopenPullRequestAsync(pullRequest),
                _resources.GetResource("Message_ReopenPullRequest_Success"),
                _resources.GetResource("Message_ReopenPullRequest_Error"),
                _mediator);

            items.Add(new CommandContextItem(reopen));
        }

        return items;
    }
}
