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

        var user = await _client.User.Current();

        var apiOptions = new ApiOptions
        {
            PageSize = 100,
            PageCount = 1,
            StartPage = 1,
        };

        var repos = await _client.Repository.GetAllForCurrent(
            new RepositoryRequest
            {
                Affiliation = RepositoryAffiliation.OwnerAndCollaborator,
            },
            apiOptions);
        repositories.AddRange(repos);

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

    private sealed class RepositoryInfo
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
