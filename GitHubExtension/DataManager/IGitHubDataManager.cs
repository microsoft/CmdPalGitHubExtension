// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataModel;

namespace GitHubExtension;

public interface IGitHubDataManager : IDisposable
{
    DataStoreOptions DataStoreOptions { get; }

    DateTime LastUpdated { get; set; }

    Task UpdateAllDataForRepositoryAsync(string owner, string name, RequestOptions? options = null);

    Task UpdateAllDataForRepositoryAsync(string fullName, RequestOptions? options = null);

    Task UpdatePullRequestsForRepositoryAsync(string owner, string name, RequestOptions? options = null);

    Task UpdatePullRequestsForRepositoryAsync(string fullName, RequestOptions? options = null);

    Task UpdateIssuesForRepositoryAsync(string owner, string name, RequestOptions? options = null);

    Task UpdateIssuesForRepositoryAsync(string fullName, RequestOptions? options = null);

    Task UpdatePullRequestsForLoggedInDeveloperIdsAsync();

    Task UpdateReleasesForRepositoryAsync(string owner, string name, RequestOptions? options = null);

    IEnumerable<Repository> GetRepositories();

    IEnumerable<User> GetDeveloperUsers();

    IEnumerable<Notification> GetNotifications(DateTime? since = null, bool includeToasted = false);

    Repository? GetRepository(string owner, string name);

    Repository? GetRepository(string fullName);

    Task UpdateIssuesForRepositoriesAsync(Octokit.RepositoryCollection repoCollection, RequestOptions requestOptions);

    Task UpdatePullRequestsForRepositoriesAsync(Octokit.RepositoryCollection repoCollection, RequestOptions requestOptions);

    Task UpdateAllDataForRepositoriesAsync(Octokit.RepositoryCollection repoCollection, RequestOptions requestOptions);
}
