// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Client;
using Octokit;
using Serilog;

namespace GitHubExtension;

public class GitHubRepositoryHelper
{
    private static readonly Lazy<GitHubRepositoryHelper> _instance = new(() => new GitHubRepositoryHelper(GitHubClientProvider.Instance.GetClient()));

    private GitHubClient _client;

#pragma warning disable IDE0044 // Add readonly modifier
    private List<Repository> _repositories;
#pragma warning restore IDE0044 // Add readonly modifier

    private GitHubRepositoryHelper(GitHubClient client)
    {
        _client = client;
        _repositories = new List<Repository>();
    }

    public static GitHubRepositoryHelper Instance => _instance.Value;

    public void UpdateClient(GitHubClient client)
    {
        _client = client;
    }

    public async Task<List<Repository>> GetUserRepositoriesAsync()
    {
        try
        {
            var repositories = new List<Repository>();

            var user = await _client.User.Current();

            // Define the pagination options
            var apiOptions = new ApiOptions
            {
                PageSize = 100, // Number of repositories per page
                PageCount = 1,  // Number of pages to fetch at a time
                StartPage = 1,   // Starting page
            };

            // Fetch repositories where the user is owner or collaborator
            var personalRepos = await _client.Repository.GetAllForCurrent(
                new RepositoryRequest
                {
                    Affiliation = RepositoryAffiliation.OwnerAndCollaborator,
                },
                apiOptions);
            repositories.AddRange(personalRepos);

            // Remove duplicate repositories by grouping by Id
            repositories = repositories.GroupBy(repo => repo.Id).Select(group => group.First()).ToList();
            return repositories;
        }
        catch (Exception ex)
        {
            Log.Error($"Error getting user repositories: {ex}");
            return new List<Repository>();
        }
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

    public async Task<List<Repository>> GetUserAndOrganizationRepositoryCollection()
    {
        var organizationRepositoryCollection = new List<Repository>();

        // Define the pagination options
        var apiOptions = new ApiOptions
        {
            PageSize = 100, // Number of repositories per page
            PageCount = 1,  // Number of pages to fetch at a time
            StartPage = 1,   // Starting page
        };

        // Fetch repositories where the user is owner or collaborator
        var organizationRepos = await _client.Repository.GetAllForCurrent(
            new RepositoryRequest
            {
                Affiliation = RepositoryAffiliation.OrganizationMember,
            },
            apiOptions);
        organizationRepositoryCollection.AddRange(organizationRepos);

        return organizationRepositoryCollection;
    }

    public async Task<List<Organization>> GetUserOrganizationsAsync()
    {
        try
        {
            var organizations = await _client.Organization.GetAllForCurrent();
            return organizations.ToList();
        }
        catch (Exception ex)
        {
            Log.Error($"Error getting user organizations: {ex}");
            return new List<Organization>();
        }
    }

    public List<Repository> GetUserRepositories()
    {
        List<Repository> repositories = GetUserRepositoriesAsync().Result;

        foreach (var repo in repositories)
        {
            if (!_repositories.Any(r => r.Id == repo.Id))
            {
                _repositories.Add(repo);
            }
        }

        return _repositories;
    }

    public List<Repository> AddRepository(string owner, string repo)
    {
        var repository = GetGitHubRepository(owner, repo).Result;

        // check for duplicates
        if (_repositories.Any(repo => repo.Id == repository.Id))
        {
            return _repositories;
        }
        else
        {
            _repositories.Add(repository);
        }

        return _repositories;
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

    public async Task<Repository> GetGitHubRepository(string owner, string repo)
    {
        return await _client.Repository.Get(owner, repo);
    }
}
