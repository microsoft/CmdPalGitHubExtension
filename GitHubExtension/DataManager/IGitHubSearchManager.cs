// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataManager;
using GitHubExtension.DeveloperId;

namespace GitHubExtension;

public interface IGitHubSearchManager : IDisposable
{
    Task SearchForGitHubIssuesOrPRs(Octokit.SearchIssuesRequest request, string initiator, SearchCategory category, RequestOptions? options = null);

    Task SearchForGitHubIssuesOrPRs(Octokit.SearchIssuesRequest request, string initiator, SearchCategory category, IDeveloperId developerId, RequestOptions? options = null);
}
