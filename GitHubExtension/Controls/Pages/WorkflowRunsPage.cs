// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls.Commands;
using GitHubExtension.DataManager;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Serilog;

namespace GitHubExtension.Controls.Pages;

// Repository-scoped workflow runs page. Because workflow runs require a
// repository, the user types owner/repo into the search box and the page loads
// the most recent runs for that repository. Per-run actions include re-run
// (non-destructive) and cancel (destructive, confirmed), plus open in browser
// and copy URL.
public sealed partial class WorkflowRunsPage : DynamicListPage
{
    private readonly IWorkflowRunsDataManager _dataManager;
    private readonly WorkflowRunsMediator _mediator;
    private readonly IResources _resources;
    private readonly ILogger _log;

    public WorkflowRunsPage(IWorkflowRunsDataManager dataManager, WorkflowRunsMediator mediator, IResources resources)
    {
        _dataManager = dataManager;
        _mediator = mediator;
        _resources = resources;
        _log = Log.ForContext("SourceContext", $"Pages/{nameof(WorkflowRunsPage)}");

        Name = resources.GetResource("Pages_WorkflowRuns");
        Icon = GitHubIcon.IconDictionary["Workflows"];
        PlaceholderText = resources.GetResource("Pages_WorkflowRuns_Placeholder");

        _mediator.WorkflowRunsChanged += OnWorkflowRunsChanged;
    }

    private void OnWorkflowRunsChanged(object? sender, object? args)
    {
        RaiseItemsChanged(0);
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        RaiseItemsChanged(0);
    }

    public override IListItem[] GetItems() => DoGetItems(SearchText).GetAwaiter().GetResult();

    private async Task<IListItem[]> DoGetItems(string query)
    {
        var repository = (query ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(repository))
        {
            return new IListItem[]
            {
                new ListItem(new NoOpCommand())
                {
                    Title = _resources.GetResource("Pages_WorkflowRuns_Prompt"),
                    Icon = GitHubIcon.IconDictionary["Workflows"],
                },
            };
        }

        try
        {
            var runs = (await _dataManager.GetWorkflowRunsAsync(repository)).ToList();

            if (runs.Count == 0)
            {
                return new IListItem[]
                {
                    new ListItem(new NoOpCommand())
                    {
                        Title = _resources.GetResource("Pages_WorkflowRuns_None"),
                        Icon = GitHubIcon.IconDictionary["Workflows"],
                    },
                };
            }

            return runs.Select(run => GetListItem(repository, run)).ToArray();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to load workflow runs.");
            return new IListItem[]
            {
                new ListItem(new NoOpCommand())
                {
                    Title = _resources.GetResource("Pages_Error_Title"),
                    Details = new Details()
                    {
                        Title = ex.Message,
                        Body = string.IsNullOrEmpty(ex.StackTrace) ? "There is no stack trace for the error." : ex.StackTrace,
                    },
                },
            };
        }
    }

    private ListItem GetListItem(string repository, IWorkflowRun run)
    {
        var rerunCommand = new WorkflowRunActionCommand(
            _resources.GetResource("Commands_RerunWorkflow"),
            new IconInfo("\uE72C"),
            () => _dataManager.RerunAsync(repository, run.Id),
            _resources.GetResource("Message_RerunWorkflow_Success"),
            _resources.GetResource("Message_RerunWorkflow_Error"),
            _mediator);

        var cancelInner = new WorkflowRunActionCommand(
            _resources.GetResource("Commands_CancelWorkflow"),
            new IconInfo("\uE711"),
            () => _dataManager.CancelAsync(repository, run.Id),
            _resources.GetResource("Message_CancelWorkflow_Success"),
            _resources.GetResource("Message_CancelWorkflow_Error"),
            _mediator);

        var cancelCommand = new ConfirmedCommand(
            cancelInner,
            _resources.GetResource("Commands_CancelWorkflow"),
            new IconInfo("\uE711"),
            _resources.GetResource("Confirm_CancelWorkflow_Title"),
            _resources.GetResource("Confirm_CancelWorkflow_Description"));

        var moreCommands = new List<CommandContextItem>
        {
            new(rerunCommand),
        };

        if (IsCancellable(run.Status))
        {
            moreCommands.Add(new CommandContextItem(cancelCommand) { IsCritical = true });
        }

        moreCommands.Add(new CommandContextItem(new CopyCommand(run.HtmlUrl, _resources.GetResource("Commands_CopyURL"), _resources)));

        return new ListItem(new LinkCommand(run.HtmlUrl, _resources))
        {
            Title = GetTitle(run),
            Subtitle = GetSubtitle(run),
            Icon = GitHubIcon.IconDictionary["Workflows"],
            MoreCommands = moreCommands.ToArray(),
        };
    }

    private string GetTitle(IWorkflowRun run)
    {
        var name = string.IsNullOrEmpty(run.Name) ? _resources.GetResource("Pages_WorkflowRuns_UnnamedRun") : run.Name;
        return $"{name} #{run.RunNumber}";
    }

    private string GetSubtitle(IWorkflowRun run)
    {
        var state = string.IsNullOrEmpty(run.Conclusion) ? run.Status : run.Conclusion;
        return string.IsNullOrEmpty(run.HeadBranch) ? state : $"{run.HeadBranch} - {state}";
    }

    private static bool IsCancellable(string status)
    {
        return status.Equals("queued", StringComparison.OrdinalIgnoreCase)
            || status.Equals("in_progress", StringComparison.OrdinalIgnoreCase)
            || status.Equals("InProgress", StringComparison.OrdinalIgnoreCase)
            || status.Equals("Queued", StringComparison.OrdinalIgnoreCase)
            || status.Equals("requested", StringComparison.OrdinalIgnoreCase)
            || status.Equals("waiting", StringComparison.OrdinalIgnoreCase)
            || status.Equals("pending", StringComparison.OrdinalIgnoreCase);
    }
}
