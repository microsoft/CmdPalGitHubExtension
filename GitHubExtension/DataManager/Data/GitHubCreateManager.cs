// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Client;
using GitHubExtension.Controls;
using Octokit;
using Serilog;

namespace GitHubExtension.DataManager.Data;

public sealed class GitHubCreateManager : IGitHubCreateManager
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", nameof(GitHubCreateManager)));

    private static readonly ILogger _log = _logger.Value;

    private readonly GitHubClientProvider _gitHubClientProvider;

    public GitHubCreateManager(GitHubClientProvider gitHubClientProvider)
    {
        _gitHubClientProvider = gitHubClientProvider;
    }

    public async Task<string> CreateIssueAsync(string ownerRepo, string title, string body)
    {
        var (owner, repo) = ParseOwnerRepo(ownerRepo);
        var client = await _gitHubClientProvider.GetClientForLoggedInDeveloper(false);
        var newIssue = new NewIssue(title) { Body = body };
        var created = await client.Issue.Create(owner, repo, newIssue);
        _log.Information($"Created issue {owner}/{repo}#{created.Number}.");
        return created.HtmlUrl;
    }

    public async Task<string> CreatePullRequestAsync(string ownerRepo, string title, string headBranch, string baseBranch, string body)
    {
        var (owner, repo) = ParseOwnerRepo(ownerRepo);
        var client = await _gitHubClientProvider.GetClientForLoggedInDeveloper(false);
        var newPullRequest = new NewPullRequest(title, headBranch, baseBranch) { Body = body };
        var created = await client.PullRequest.Create(owner, repo, newPullRequest);
        _log.Information($"Created pull request {owner}/{repo}#{created.Number}.");
        return created.HtmlUrl;
    }

    public async Task<string> CreateBranchAsync(string ownerRepo, string newBranch, string sourceBranch)
    {
        var (owner, repo) = ParseOwnerRepo(ownerRepo);
        var client = await _gitHubClientProvider.GetClientForLoggedInDeveloper(false);

        var sourceReference = await client.Git.Reference.Get(owner, repo, $"heads/{sourceBranch}");
        var newReference = new NewReference($"refs/heads/{newBranch}", sourceReference.Object.Sha);
        var created = await client.Git.Reference.Create(owner, repo, newReference);
        _log.Information($"Created branch {newBranch} in {owner}/{repo} from {sourceBranch}.");
        return created.Ref;
    }

    public async Task AddCommentAsync(IIssue issue, string comment)
    {
        var owner = Validation.ParseOwnerFromGitHubURL(issue.HtmlUrl);
        var repo = Validation.ParseRepositoryFromGitHubURL(issue.HtmlUrl);
        var client = await _gitHubClientProvider.GetClientForLoggedInDeveloper(false);
        await client.Issue.Comment.Create(owner, repo, (int)issue.Number, comment);
        _log.Information($"Added comment to {owner}/{repo}#{issue.Number}.");
    }

    internal static (string Owner, string Repo) ParseOwnerRepo(string ownerRepo)
    {
        if (string.IsNullOrWhiteSpace(ownerRepo))
        {
            throw new ArgumentException("Repository must be provided as owner/repo.", nameof(ownerRepo));
        }

        var trimmed = ownerRepo.Trim();

        // Accept a full GitHub URL as well as the short owner/repo form.
        if (Validation.IsValidHttpUri(trimmed, out var uri) && uri != null)
        {
            return (Validation.ParseOwnerFromGitHubURL(uri), Validation.ParseRepositoryFromGitHubURL(uri));
        }

        var parts = trimmed.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            throw new ArgumentException("Repository must be in the form owner/repo.", nameof(ownerRepo));
        }

        return (parts[0], parts[1]);
    }
}
