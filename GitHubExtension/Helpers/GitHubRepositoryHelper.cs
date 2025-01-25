// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Octokit;
using Serilog;

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

            // Fetch repositories where the user is a collaborator with pagination
            var collaboratorRepos = await _client.Repository.GetAllForCurrent(
                new RepositoryRequest
                {
                    Affiliation = RepositoryAffiliation.OwnerAndCollaborator,
                },
                apiOptions);
            repositories.AddRange(collaboratorRepos);

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
}
