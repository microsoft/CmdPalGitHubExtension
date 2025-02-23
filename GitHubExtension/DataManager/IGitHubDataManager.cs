// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataManager;
using GitHubExtension.DataModel;
using GitHubExtension.DataModel.Enums;

namespace GitHubExtension;

public interface IGitHubDataManager : IDisposable
{
    DataStoreOptions DataStoreOptions { get; }

    DateTime LastUpdated { get; set; }

    public event DataManagerUpdateEventHandler? OnUpdate;

    Task UpdateAllDataForRepositoryAsync(string owner, string name, RequestOptions? options = null);

    Task UpdateAllDataForRepositoryAsync(string fullName, RequestOptions? options = null);

    Task UpdatePullRequestsForRepositoryAsync(string owner, string name, RequestOptions? options = null);

    Task UpdatePullRequestsForRepositoryAsync(string fullName, RequestOptions? options = null);

    Task UpdateIssuesForRepositoryAsync(string owner, string name, RequestOptions? options = null);

    Task UpdateIssuesForRepositoryAsync(string fullName, RequestOptions? options = null);

    IEnumerable<Repository> GetRepositories();

    IEnumerable<User> GetDeveloperUsers();

    Repository? GetRepository(string owner, string name);

    Repository? GetRepository(string fullName);

    Task UpdateIssuesForRepositoriesAsync(Octokit.RepositoryCollection repoCollection, RequestOptions requestOptions);

    Task UpdatePullRequestsForRepositoriesAsync(Octokit.RepositoryCollection repoCollection, RequestOptions requestOptions);

    Task UpdateAllDataForRepositoriesAsync(Octokit.RepositoryCollection repoCollection, RequestOptions requestOptions);

    Task UpdateDataForSearchAsync(string name, string searchString, SearchType type, RequestOptions options);

    Task UpdateDataForSearchesAsync(IEnumerable<PersistentData.Search> searches, RequestOptions options);

    Task RequestAllUpdateAsync(Octokit.RepositoryCollection repoCollection, List<PersistentData.Search> searches, RequestOptions options);

    Task RequestIssuesUpdateAsync(Octokit.RepositoryCollection repoCollection, RequestOptions options);

    Task RequestPullRequestsUpdateAsync(Octokit.RepositoryCollection repoCollection, RequestOptions options);

    Task RequestSearchUpdateAsync(string name, string searchString, SearchType type, RequestOptions options);

    Search? GetSearch(string name, string searchString);
}
