// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;

namespace GitHubExtension.DataManager;

public interface IGitHubAutoMergeManager
{
    // Enables auto-merge on a pull request. GraphQL only.
    Task EnableAutoMergeAsync(IPullRequest pullRequest);

    // Disables auto-merge on a pull request. GraphQL only.
    Task DisableAutoMergeAsync(IPullRequest pullRequest);
}
