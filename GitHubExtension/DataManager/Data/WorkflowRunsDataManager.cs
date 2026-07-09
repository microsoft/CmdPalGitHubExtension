// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Client;
using GitHubExtension.Controls;
using Octokit;
using Serilog;

namespace GitHubExtension.DataManager.Data;

public sealed class WorkflowRunsDataManager : IWorkflowRunsDataManager
{
    private const int DefaultPageSize = 30;

    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", nameof(WorkflowRunsDataManager)));

    private static readonly ILogger _log = _logger.Value;

    private readonly GitHubClientProvider _gitHubClientProvider;

    public WorkflowRunsDataManager(GitHubClientProvider gitHubClientProvider)
    {
        _gitHubClientProvider = gitHubClientProvider;
    }

    public async Task<IEnumerable<IWorkflowRun>> GetWorkflowRunsAsync(string ownerRepo)
    {
        var (owner, repo) = GitHubCreateManager.ParseOwnerRepo(ownerRepo);
        var client = await _gitHubClientProvider.GetClientForLoggedInDeveloper(false);

        var options = new ApiOptions { PageCount = 1, PageSize = DefaultPageSize, StartPage = 1 };
        var response = await client.Actions.Workflows.Runs.List(owner, repo, new WorkflowRunsRequest(), options);

        _log.Debug($"Retrieved {response.WorkflowRuns.Count} workflow runs for {owner}/{repo}.");

        return response.WorkflowRuns.Select(run => ToWorkflowRun(run, $"{owner}/{repo}")).ToList();
    }

    public async Task RerunAsync(string ownerRepo, long runId)
    {
        var (owner, repo) = GitHubCreateManager.ParseOwnerRepo(ownerRepo);
        var client = await _gitHubClientProvider.GetClientForLoggedInDeveloper(false);
        await client.Actions.Workflows.Runs.Rerun(owner, repo, runId);
        _log.Information($"Re-ran workflow run {runId} in {owner}/{repo}.");
    }

    public async Task CancelAsync(string ownerRepo, long runId)
    {
        var (owner, repo) = GitHubCreateManager.ParseOwnerRepo(ownerRepo);
        var client = await _gitHubClientProvider.GetClientForLoggedInDeveloper(false);
        await client.Actions.Workflows.Runs.Cancel(owner, repo, runId);
        _log.Information($"Cancelled workflow run {runId} in {owner}/{repo}.");
    }

    public async Task TriggerWorkflowAsync(string ownerRepo, string workflowFileName, string gitRef)
    {
        var (owner, repo) = GitHubCreateManager.ParseOwnerRepo(ownerRepo);
        var client = await _gitHubClientProvider.GetClientForLoggedInDeveloper(false);
        await client.Actions.Workflows.CreateDispatch(owner, repo, workflowFileName, new CreateWorkflowDispatch(gitRef));
        _log.Information($"Triggered workflow {workflowFileName} on {gitRef} in {owner}/{repo}.");
    }

    private static Controls.WorkflowRun ToWorkflowRun(Octokit.WorkflowRun run, string repositoryFullName)
    {
        return new Controls.WorkflowRun
        {
            Id = run.Id,
            Name = run.Name ?? string.Empty,
            Status = run.Status.StringValue ?? string.Empty,
            Conclusion = run.Conclusion.HasValue ? (run.Conclusion.Value.StringValue ?? string.Empty) : string.Empty,
            HeadBranch = run.HeadBranch ?? string.Empty,
            TriggerEvent = run.Event ?? string.Empty,
            RunNumber = run.RunNumber,
            HtmlUrl = run.HtmlUrl ?? string.Empty,
            RepositoryFullName = repositoryFullName,
            CreatedAt = run.CreatedAt,
        };
    }
}
