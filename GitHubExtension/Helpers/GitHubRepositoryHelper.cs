// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
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

    public async Task<Repository> GetSavedRepositoryAsync()
    {
        var json = File.ReadAllText("repositoryInfo.json");
        var repoInfo = JsonSerializer.Deserialize<RepositoryInfo>(json);

        if (repoInfo == null)
        {
            throw new InvalidOperationException("No repository information found.");
        }

        var repository = await _client.Repository.Get(repoInfo.Owner, repoInfo.Name);
        return repository;
    }

    public async Task<List<Issue>> GetRepositoryIssuesAsync()
    {
        var repository = await GetSavedRepositoryAsync();
        var issues = await _client.Issue.GetAllForRepository(repository.Owner.Login, repository.Name);
        return issues.ToList();
    }

    private class RepositoryInfo
    {
        private string? url = string.Empty;

        public string? Name { get; set; }

        public string Url
        {
            get => url ?? string.Empty;
            set => url = value;
        }

        public string Owner => Url.Split('/')[3]; // Assuming the URL is in the format https://github.com/owner/repo
    }
}
