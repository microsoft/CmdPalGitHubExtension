// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Client;
using GitHubExtension.PersistentData;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Octokit;
using Serilog;

namespace GitHubExtension;

public class GitHubRepositoryHelper
{
    private static readonly Lazy<GitHubRepositoryHelper> _instance = new(() => new GitHubRepositoryHelper(GitHubClientProvider.Instance.GetClient()));

    private GitHubClient _client;

    private GitHubRepositoryHelper(GitHubClient client)
    {
        _client = client;
        _ = AddUsersContribuitionRepositoriesToDatabaseAsync();
    }

    public static GitHubRepositoryHelper Instance => _instance.Value;

    public void UpdateClient(GitHubClient client)
    {
        _client = client;
        _ = AddUsersContribuitionRepositoriesToDatabaseAsync();
    }

    // TODO: Fix this. Auth issues. Maybe calling way too early.
    public async Task<List<Octokit.Repository>> GetUserRepositoriesFromOctokitAsync()
    {
        // band-aid for now: check if the client has a user
        if (GitHubClientProvider.Instance.IsClientLoggedIn(_client) == false)
        {
            return new List<Octokit.Repository>();
        }

        try
        {
            var repositories = new List<Octokit.Repository>();

            var user = await _client.User.Current();

            var apiOptions = new ApiOptions
            {
                PageSize = 100,
                PageCount = 1,
                StartPage = 1,
            };

            var personalRepos = await _client.Repository.GetAllForCurrent(
                new RepositoryRequest
                {
                    Affiliation = RepositoryAffiliation.OwnerAndCollaborator,
                },
                apiOptions);
            repositories.AddRange(personalRepos);

            repositories = repositories.GroupBy(repo => repo.Id).Select(group => group.First()).ToList();
            return repositories;
        }
        catch (Exception ex)
        {
            Log.Error($"Error getting user repositories: {ex}");
            return new List<Octokit.Repository>();
        }
    }

    public async Task AddUsersContribuitionRepositoriesToDatabaseAsync()
    {
        var repositories = await GetUserRepositoriesFromOctokitAsync();
        var dataManager = PersistentDataManager.CreateInstance();
        foreach (var repo in repositories)
        {
            try
            {
                await dataManager!.AddRepositoryAsync(repo.Owner.Login, repo.Name);
            }
            catch (Exception ex)
            {
                Log.Error($"Error adding user's repositories to database: {ex}");
            }
        }
    }

    public RepositoryCollection GetUserRepositoryCollection()
    {
        var repositoryCollection = new RepositoryCollection();
        var repositories = GetUserRepositoriesAsync().Result;

        foreach (var repo in repositories)
        {
            repositoryCollection.Add($"{repo.OwnerLogin}/{repo.Name}");
        }

        return repositoryCollection;
    }

    public async Task<IEnumerable<PersistentData.Repository>> GetUserRepositoriesAsync()
    {
        PersistentDataManager? dataManager = PersistentDataManager.CreateInstance();
        return await dataManager!.GetAllRepositoriesAsync();
    }

    public async Task AddRepository(string owner, string repo)
    {
        PersistentDataManager? dataManager = PersistentDataManager.CreateInstance();
        await dataManager!.AddRepositoryAsync(owner, repo);
    }

    public bool IsMemberOrContributor(string ownerName, string repositoryName)
    {
        var userName = _client.User.Current().Result.Login;

        var isMember = IsUserMemberOfRepository(ownerName, repositoryName, userName).Result;
        var isContributor = IsUserContributorOfRepository(ownerName, repositoryName, userName).Result;

        return isMember || isContributor;
    }

    private async Task<bool> IsUserMemberOfRepository(string ownerName, string repositoryName, string userName)
    {
        try
        {
            var collaborators = await _client.Issue.Assignee.GetAllForRepository(ownerName, repositoryName);
            if (collaborators.Count == 0)
            {
                return false;
            }
            else
            {
                return collaborators.Any(collaborator => collaborator.Login == userName);
            }
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage(new LogMessage() { Message = $"Error in IsUserMemberOfRepository: {ex.Message}" });
            throw;
        }
    }

    private async Task<bool> IsUserContributorOfRepository(string ownerName, string repositoryName, string userName)
    {
        try
        {
            var commits = await _client.Repository.Commit.GetAll(ownerName, repositoryName, new CommitRequest { Author = userName });
            var issueSearchRequest = new SearchIssuesRequest(repositoryName)
            {
                Author = userName,
                Type = IssueTypeQualifier.PullRequest,
                SortField = IssueSearchSort.Created,
                Order = SortDirection.Descending,
            };

            var pullRequests = await _client.Search.SearchIssues(issueSearchRequest);

            return (commits.Count > 0) || (pullRequests.TotalCount > 0);
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage(new LogMessage() { Message = $"Error in IsUserContributorOfRepository: {ex.Message}" });
            throw;
        }
    }

    public void ClearRepositories()
    {
        // TODO: Implement deleting from the data store
    }
}
