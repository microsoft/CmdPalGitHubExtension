// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls.Forms;
using GitHubExtension.Controls.Pages;
using GitHubExtension.DataManager;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Commands;

// Builds the set of write-action context items for an issue or pull request.
// The available actions depend on the item's current state (open vs closed).
public sealed class MutationCommandsFactory
{
    private readonly IGitHubMutationManager _mutationManager;
    private readonly IGitHubCreateManager _createManager;
    private readonly IGitHubAutoMergeManager _autoMergeManager;
    private readonly MutationMediator _mediator;
    private readonly IResources _resources;

    private static readonly IconInfo CloseIcon = new("\uE711");
    private static readonly IconInfo ReopenIcon = new("\uE72C");
    private static readonly IconInfo MergeIcon = new("\uE8FB");
    private static readonly IconInfo AutoMergeIcon = new("\uE945");

    public MutationCommandsFactory(IGitHubMutationManager mutationManager, IGitHubCreateManager createManager, IGitHubAutoMergeManager autoMergeManager, MutationMediator mediator, IResources resources)
    {
        _mutationManager = mutationManager;
        _createManager = createManager;
        _autoMergeManager = autoMergeManager;
        _mediator = mediator;
        _resources = resources;
    }

    private static bool IsOpen(IIssue issue) => string.Equals(issue.State, "Open", StringComparison.OrdinalIgnoreCase);

    private CommandContextItem BuildAddCommentCommand(IIssue issue)
    {
        var page = new GitHubFormPage(
            new AddCommentForm(issue, _createManager, _resources),
            _resources,
            "Forms_AddComment_Title",
            "\uE90A",
            "Message_AddComment_Success",
            "Message_AddComment_Error");

        return new CommandContextItem(page);
    }

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

        items.Add(BuildAddCommentCommand(issue));

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

            var enableAutoMerge = new GitHubMutationCommand(
                _resources.GetResource("Commands_EnableAutoMerge"),
                AutoMergeIcon,
                () => _autoMergeManager.EnableAutoMergeAsync(pullRequest),
                _resources.GetResource("Message_EnableAutoMerge_Success"),
                _resources.GetResource("Message_EnableAutoMerge_Error"),
                _mediator);

            items.Add(new CommandContextItem(enableAutoMerge));

            var disableAutoMerge = new GitHubMutationCommand(
                _resources.GetResource("Commands_DisableAutoMerge"),
                AutoMergeIcon,
                () => _autoMergeManager.DisableAutoMergeAsync(pullRequest),
                _resources.GetResource("Message_DisableAutoMerge_Success"),
                _resources.GetResource("Message_DisableAutoMerge_Error"),
                _mediator);

            items.Add(new CommandContextItem(disableAutoMerge));
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

        items.Add(BuildAddCommentCommand(pullRequest));

        return items;
    }
}
