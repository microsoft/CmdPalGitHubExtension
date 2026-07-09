// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;

namespace GitHubExtension.DataManager;

// Performs write operations against existing issues and pull requests. Kept
// separate from the read-only cache/data-requester path. Shared by M3 (state
// changes) and reused by later create/workflow milestones.
public interface IGitHubMutationManager
{
    Task CloseIssueAsync(IIssue issue);

    Task ReopenIssueAsync(IIssue issue);

    Task ClosePullRequestAsync(IPullRequest pullRequest);

    Task ReopenPullRequestAsync(IPullRequest pullRequest);

    Task MergePullRequestAsync(IPullRequest pullRequest);
}
