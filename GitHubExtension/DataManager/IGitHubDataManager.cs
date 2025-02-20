// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataModel;
using GitHubExtension.DataModel.Enums;

namespace GitHubExtension;

public interface IGitHubDataManager : IDisposable
{
    DataStoreOptions DataStoreOptions { get; }

    DateTime LastUpdated { get; set; }

    IEnumerable<Repository> GetRepositories();

    IEnumerable<User> GetDeveloperUsers();

    Repository? GetRepository(string owner, string name);

    Repository? GetRepository(string fullName);

    Task RequestAllUpdateAsync(Octokit.RepositoryCollection repoCollection, List<PersistentData.Search> searches, RequestOptions options);

    Task RequestIssuesUpdateAsync(Octokit.RepositoryCollection repoCollection, RequestOptions options);

    Task RequestPullRequestsUpdateAsync(Octokit.RepositoryCollection repoCollection, RequestOptions options);

    Task RequestSearchUpdateAsync(string name, string searchString, SearchType type, RequestOptions options);

    Search? GetSearch(string name, string searchString);
}
