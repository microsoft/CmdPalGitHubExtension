// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Client;
using GitHubExtension.Controls;
using Octokit;
using Serilog;

namespace GitHubExtension.DataManager.Data;

public sealed class GitHubMutationManager : IGitHubMutationManager
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", nameof(GitHubMutationManager)));

    private static readonly ILogger _log = _logger.Value;

    private readonly GitHubClientProvider _gitHubClientProvider;

    public GitHubMutationManager(GitHubClientProvider gitHubClientProvider)
    {
        _gitHubClientProvider = gitHubClientProvider;
    }

    public async Task CloseIssueAsync(IIssue issue)
    {
        var (owner, repo, number) = Parse(issue);
        var client = await _gitHubClientProvider.GetClientForLoggedInDeveloper(false);
        await client.Issue.Update(owner, repo, number, new IssueUpdate { State = ItemState.Closed });
        _log.Information($"Closed issue {owner}/{repo}#{number}.");
    }

    public async Task ReopenIssueAsync(IIssue issue)
    {
        var (owner, repo, number) = Parse(issue);
        var client = await _gitHubClientProvider.GetClientForLoggedInDeveloper(false);
        await client.Issue.Update(owner, repo, number, new IssueUpdate { State = ItemState.Open });
        _log.Information($"Reopened issue {owner}/{repo}#{number}.");
    }

    public async Task ClosePullRequestAsync(IPullRequest pullRequest)
    {
        var (owner, repo, number) = Parse(pullRequest);
        var client = await _gitHubClientProvider.GetClientForLoggedInDeveloper(false);
        await client.PullRequest.Update(owner, repo, number, new PullRequestUpdate { State = ItemState.Closed });
        _log.Information($"Closed pull request {owner}/{repo}#{number}.");
    }

    public async Task ReopenPullRequestAsync(IPullRequest pullRequest)
    {
        var (owner, repo, number) = Parse(pullRequest);
        var client = await _gitHubClientProvider.GetClientForLoggedInDeveloper(false);
        await client.PullRequest.Update(owner, repo, number, new PullRequestUpdate { State = ItemState.Open });
        _log.Information($"Reopened pull request {owner}/{repo}#{number}.");
    }

    public async Task MergePullRequestAsync(IPullRequest pullRequest)
    {
        var (owner, repo, number) = Parse(pullRequest);
        var client = await _gitHubClientProvider.GetClientForLoggedInDeveloper(false);
        await client.PullRequest.Merge(owner, repo, number, new MergePullRequest());
        _log.Information($"Merged pull request {owner}/{repo}#{number}.");
    }

    private static (string Owner, string Repo, int Number) Parse(IIssue issue)
    {
        var owner = Validation.ParseOwnerFromGitHubURL(issue.HtmlUrl);
        var repo = Validation.ParseRepositoryFromGitHubURL(issue.HtmlUrl);
        return (owner, repo, (int)issue.Number);
    }
}
