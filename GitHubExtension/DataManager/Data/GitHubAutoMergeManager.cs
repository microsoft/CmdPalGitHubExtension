// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Client;
using GitHubExtension.Controls;
using Serilog;

namespace GitHubExtension.DataManager.Data;

// Enables and disables pull-request auto-merge. Auto-merge is only available
// through the GraphQL API, so this manager resolves the pull request's global
// node id and then issues the corresponding GraphQL mutation.
public sealed class GitHubAutoMergeManager : IGitHubAutoMergeManager
{
    private const string PullRequestIdQuery = @"
query($owner: String!, $repo: String!, $number: Int!) {
  repository(owner: $owner, name: $repo) {
    pullRequest(number: $number) {
      id
    }
  }
}";

    private const string EnableAutoMergeMutation = @"
mutation($id: ID!) {
  enablePullRequestAutoMerge(input: {pullRequestId: $id}) {
    clientMutationId
  }
}";

    private const string DisableAutoMergeMutation = @"
mutation($id: ID!) {
  disablePullRequestAutoMerge(input: {pullRequestId: $id}) {
    clientMutationId
  }
}";

    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", nameof(GitHubAutoMergeManager)));

    private static readonly ILogger _log = _logger.Value;

    private readonly IGitHubGraphQLClient _graphQLClient;

    public GitHubAutoMergeManager(IGitHubGraphQLClient graphQLClient)
    {
        _graphQLClient = graphQLClient;
    }

    public async Task EnableAutoMergeAsync(IPullRequest pullRequest)
    {
        var id = await GetPullRequestIdAsync(pullRequest);
        await _graphQLClient.QueryAsync(EnableAutoMergeMutation, new Dictionary<string, object?> { ["id"] = id });
        _log.Information($"Enabled auto-merge for {pullRequest.HtmlUrl}.");
    }

    public async Task DisableAutoMergeAsync(IPullRequest pullRequest)
    {
        var id = await GetPullRequestIdAsync(pullRequest);
        await _graphQLClient.QueryAsync(DisableAutoMergeMutation, new Dictionary<string, object?> { ["id"] = id });
        _log.Information($"Disabled auto-merge for {pullRequest.HtmlUrl}.");
    }

    private async Task<string> GetPullRequestIdAsync(IPullRequest pullRequest)
    {
        var owner = Validation.ParseOwnerFromGitHubURL(pullRequest.HtmlUrl);
        var repo = Validation.ParseRepositoryFromGitHubURL(pullRequest.HtmlUrl);

        var variables = new Dictionary<string, object?>
        {
            ["owner"] = owner,
            ["repo"] = repo,
            ["number"] = (int)pullRequest.Number,
        };

        var data = await _graphQLClient.QueryAsync(PullRequestIdQuery, variables);
        var id = data?["repository"]?["pullRequest"]?["id"]?.ToString();

        if (string.IsNullOrEmpty(id))
        {
            throw new InvalidOperationException($"Could not resolve the pull request id for {pullRequest.HtmlUrl}.");
        }

        return id;
    }
}
