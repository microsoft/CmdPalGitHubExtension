// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;

namespace GitHubExtension.DataManager;

// Performs create/write operations that produce new GitHub resources (issues,
// pull requests, branches) plus comments on existing items. Kept separate from
// the read-only cache path and from the state-mutation manager. Introduced in
// M4 (create flows) and reused by later milestones.
public interface IGitHubCreateManager
{
    Task<string> CreateIssueAsync(string ownerRepo, string title, string body);

    Task<string> CreatePullRequestAsync(string ownerRepo, string title, string headBranch, string baseBranch, string body);

    Task<string> CreateBranchAsync(string ownerRepo, string newBranch, string sourceBranch);

    Task AddCommentAsync(IIssue issue, string comment);
}
