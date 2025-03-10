// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.DataManager.GitHubDataManager;
using GitHubExtension.DataModel.Enums;

namespace GitHubExtension.DataManager.Cache;

public interface IGitHubCacheDataManager
{
    DateTime LastUpdated { get; set; }

    event DataManagerUpdateEventHandler? OnUpdate;

    Task RequestAllUpdateAsync(Octokit.RepositoryCollection repoCollection, List<ISearch> searches, RequestOptions options);

    Task RequestIssuesUpdateAsync(Octokit.RepositoryCollection repoCollection, RequestOptions options);

    Task RequestPullRequestsUpdateAsync(Octokit.RepositoryCollection repoCollection, RequestOptions options);

    Task RequestSearchUpdateAsync(string name, string searchString, SearchType type, RequestOptions options);

    bool IsSearchNewOrStale(ISearch search, TimeSpan refreshCooldown);
}
