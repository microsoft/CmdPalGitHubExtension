// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Octokit;

namespace GitHubExtension;

public class GitHubRepositoryHelper
{
    private readonly GitHubClient _client;

    public GitHubRepositoryHelper(GitHubClient client)
    {
        _client = client;
    }

    public async Task<List<Repository>> GetUserRepositoriesAsync()
    {
        var repositories = new List<Repository>();

        // Get the authenticated user
        var user = await _client.User.Current();

        // Get repositories the user owns and/or contributes to
        var repos = await _client.Repository.GetAllForCurrent();
        repositories.AddRange(repos);

        // Remove duplicates
        repositories = repositories.GroupBy(repo => repo.Id).Select(group => group.First()).ToList();

        return repositories;
    }

    public async Task<RepositoryCollection> GetUserRepositoryCollection()
    {
        var repositoryCollection = new RepositoryCollection();
        var repositories = await GetUserRepositoriesAsync();

        foreach (var repo in repositories)
        {
            repositoryCollection.Add($"{repo.Owner.Login}/{repo.Name}");
        }

        return repositoryCollection;
    }
}
