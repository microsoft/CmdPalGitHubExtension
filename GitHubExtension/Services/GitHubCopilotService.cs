// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataModel.DataObjects;
using GitHubExtension.DeveloperIds;
using Serilog;

namespace GitHubExtension.Services;

public class GitHubCopilotService : IGitHubCopilotService
{
    private static readonly Lazy<ILogger> _logger = new(() => Log.ForContext("SourceContext", nameof(GitHubCopilotService)));
    private static readonly ILogger _log = _logger.Value;

    private readonly IDeveloperIdProvider _developerIdProvider;

    public GitHubCopilotService(IDeveloperIdProvider developerIdProvider)
    {
        _developerIdProvider = developerIdProvider;
    }

    public bool IsAvailable()
    {
        var devIds = _developerIdProvider.GetLoggedInDeveloperIdsInternal();
        return devIds != null && devIds.Any();
    }

    public async Task<string> SendMessageAsync(string message)
    {
        if (!IsAvailable())
        {
            return "Please sign in to GitHub to use Copilot functionality.";
        }

        try
        {
            var devIds = _developerIdProvider.GetLoggedInDeveloperIdsInternal();
            var devId = devIds?.FirstOrDefault();

            if (devId?.GitHubClient == null)
            {
                return "GitHub authentication is not available. Please sign in again.";
            }

            var response = await SimulateCopilotResponse(message);
            _log.Information($"Copilot query processed: {message.Substring(0, Math.Min(50, message.Length))}...");
            return response;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error sending message to GitHub Copilot");
            return $"Error communicating with GitHub Copilot: {ex.Message}";
        }
    }

    public async Task<IEnumerable<CopilotTask>> GetCopilotTasksAsync()
    {
        if (!IsAvailable())
        {
            _log.Warning("GitHub Copilot tasks cannot be retrieved - user not signed in");
            return Enumerable.Empty<CopilotTask>();
        }

        try
        {
            var devIds = _developerIdProvider.GetLoggedInDeveloperIdsInternal();
            var devId = devIds?.FirstOrDefault();

            if (devId?.GitHubClient == null)
            {
                _log.Warning("GitHub Copilot tasks cannot be retrieved - client not available");
                return Enumerable.Empty<CopilotTask>();
            }

            var client = devId.GitHubClient;

            // Log authentication info
            var rateLimit = await client.RateLimit.GetRateLimits();
            _log.Information("========================================");
            _log.Information($"GitHub API Rate Limits:");
            _log.Information($"  Core: {rateLimit.Resources.Core.Remaining}/{rateLimit.Resources.Core.Limit} (Resets: {rateLimit.Resources.Core.Reset.ToLocalTime():HH:mm:ss})");
            _log.Information($"  Search: {rateLimit.Resources.Search.Remaining}/{rateLimit.Resources.Search.Limit} (Resets: {rateLimit.Resources.Search.Reset.ToLocalTime():HH:mm:ss})");
            _log.Information("========================================");

            var user = await client.User.Current();
            var tasks = new List<CopilotTask>();

            _log.Information("========================================");
            _log.Information($"Fetching Copilot tasks for user: {user.Login}");
            _log.Information($"User has {user.PublicRepos} public repos, {user.TotalPrivateRepos} private repos");
            _log.Information("========================================");

            // Test authentication and permissions
            _log.Information("========================================");
            _log.Information("Testing GitHub Authentication & Permissions:");
            _log.Information($"  Authenticated User: {user.Login}");
            _log.Information($"  User ID: {user.Id}");
            _log.Information($"  Account Type: {user.Type}");
            _log.Information($"  Created: {user.CreatedAt}");

            // Check OAuth scopes
            try
            {
                var scopes = client.GetLastApiInfo()?.OauthScopes;
                if (scopes != null && scopes.Any())
                {
                    _log.Information($"  OAuth Scopes Granted:");
                    foreach (var scope in scopes)
                    {
                        _log.Information($"    - {scope}");
                    }

                    if (!scopes.Contains("repo"))
                    {
                        _log.Warning($"  ??  WARNING: 'repo' scope is NOT granted!");
                        _log.Warning($"  This is why you cannot see private repositories.");
                        _log.Warning($"  Solution: Sign out and sign in again to grant the 'repo' scope.");
                    }
                    else
                    {
                        _log.Information($"  ? 'repo' scope is granted (private repos should be accessible)");
                    }
                }
                else
                {
                    _log.Warning($"  Could not retrieve OAuth scopes from API");
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"  Error checking OAuth scopes: {ex.Message}");
            }

            // Test if we can access the specific repository
            try
            {
                var repo = await client.Repository.Get("cinnamon-msft", "time-zone-converter");
                _log.Information($"  ? Can access cinnamon-msft/time-zone-converter");
                _log.Information($"    Repo ID: {repo.Id}");
                _log.Information($"    Owner: {repo.Owner.Login}");
                _log.Information($"    Visibility: {(repo.Private ? "Private" : "Public")}");
                _log.Information($"    Permissions: Push={repo.Permissions?.Push}, Pull={repo.Permissions?.Pull}, Admin={repo.Permissions?.Admin}");
            }
            catch (Octokit.NotFoundException)
            {
                _log.Warning($"  ? Cannot access cinnamon-msft/time-zone-converter (not found or no permission)");
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"  ? Error accessing cinnamon-msft/time-zone-converter: {ex.Message}");
            }

            _log.Information("========================================");

            // Define Copilot bot accounts
            var copilotBots = new[]
            {
                "copilot-workspace[bot]",
                "github-copilot[bot]",
                "copilot[bot]",
                "Copilot", // GitHub displays this without [bot] suffix in some cases
                "copilot-workspace",
                "github-copilot",
                "copilot",
            };

            // Search strategy: Only search repositories owned by the current user
            // This avoids rate limiting by focusing on a smaller, more relevant set of repositories
            try
            {
                _log.Information("Fetching user's repositories to search for Copilot PRs...");

                // Get repositories owned by user AND in organizations they're part of
                var repoRequest = new Octokit.RepositoryRequest
                {
                    Type = Octokit.RepositoryType.All, // All accessible repos (owner + member)
                    Sort = Octokit.RepositorySort.Updated,
                    Direction = Octokit.SortDirection.Descending,
                };

                var repositories = await client.Repository.GetAllForCurrent(repoRequest);
                _log.Information($"Found {repositories.Count} repositories accessible to user");

                // Limit to most recently updated to avoid rate limits
                var reposToSearch = repositories.Take(50).ToList();
                _log.Information($"Will search {reposToSearch.Count} most recently updated repositories");

                // Search each repository for Copilot PRs
                foreach (var repo in reposToSearch)
                {
                    try
                    {
                        _log.Debug($"Searching repository: {repo.FullName}");

                        // Get pull requests directly from the repository
                        // This is more efficient than using search API
                        var prRequest = new Octokit.PullRequestRequest
                        {
                            State = Octokit.ItemStateFilter.All,
                            SortProperty = Octokit.PullRequestSort.Updated,
                            SortDirection = Octokit.SortDirection.Descending,
                        };

                        var pullRequests = await client.PullRequest.GetAllForRepository(repo.Id, prRequest);

                        foreach (var pr in pullRequests)
                        {
                            // Get the author login - handle null safely
                            var authorLogin = pr.User?.Login ?? string.Empty;

                            // Log for debugging
                            _log.Debug($"  Checking PR #{pr.Number} by {authorLogin}");

                            // Check if PR was created by a Copilot bot
                            // Do case-insensitive comparison
                            var isCopilotPr = copilotBots.Any(bot =>
                                authorLogin.Equals(bot, StringComparison.OrdinalIgnoreCase));

                            if (isCopilotPr)
                            {
                                // Skip if we already have this PR
                                if (tasks.Any(t => t.Id == $"pr-{pr.Id}"))
                                {
                                    continue;
                                }

                                var prState = pr.State.Value;
                                var isMerged = pr.Merged ? "merged" : "not merged";
                                _log.Information($"  ? Found Copilot PR #{pr.Number}: {pr.Title}");
                                _log.Information($"    Repository: {repo.FullName}");
                                _log.Information($"    State: {prState} ({isMerged})");
                                _log.Information($"    Author: {authorLogin}");
                                _log.Information($"    URL: {pr.HtmlUrl}");

                                var task = CreateTaskFromPullRequest(pr, repo.FullName, authorLogin);
                                tasks.Add(task);
                            }
                        }
                    }
                    catch (Octokit.ApiException apiEx) when (apiEx.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        // Repository might have been deleted or access revoked
                        _log.Debug($"Cannot access repository {repo.FullName} (404)");
                        continue;
                    }
                    catch (Exception ex)
                    {
                        _log.Warning(ex, $"Error accessing repository {repo.FullName}");
                        continue;
                    }
                }

                _log.Information($"Repository search complete. Found {tasks.Count} Copilot tasks.");
            }
            catch (Octokit.RateLimitExceededException rateEx)
            {
                _log.Error($"Rate limit exceeded! Limit: {rateEx.Limit}, Remaining: {rateEx.Remaining}, Reset: {rateEx.Reset.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error fetching user repositories");
            }

            _log.Information("========================================");
            _log.Information($"Search complete!");
            _log.Information($"Total unique tasks found: {tasks.Count}");
            if (tasks.Count > 0)
            {
                _log.Information($"Tasks by status:");
                var byStatus = tasks.GroupBy(t => t.Status);
                foreach (var group in byStatus)
                {
                    _log.Information($"  {group.Key}: {group.Count()}");
                }
            }

            _log.Information("========================================");

            if (tasks.Count == 0)
            {
                _log.Warning("No Copilot tasks found");
                _log.Warning($"Possible reasons:");
                _log.Warning($"  1. No Copilot PRs exist in accessible repositories");
                _log.Warning($"  2. The bot accounts haven't created any PRs");
                _log.Warning($"  3. GitHub search indexing lag (new PRs take 1-2 minutes to appear)");
                _log.Warning($"  4. Private repos require 'repo' scope (already included in auth)");
                _log.Warning($"");
                _log.Warning($"To verify manually:");
                _log.Warning($"  Visit: https://github.com/search?q=is:pr+author:copilot-workspace[bot]");
                _log.Warning($"  If you see results there, wait 2-3 minutes and try again");

                return new[]
                {
                    new CopilotTask
                    {
                        Id = "info-no-results",
                        Title = "No Copilot Tasks Found",
                        Description = CreateNoResultsMessage(user.Login),
                        Status = CopilotTaskStatus.InProgress,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Repository = "github/copilot-info",
                        Url = "https://copilot-workspace.githubnext.com/",
                        Agent = "GitHub Copilot Information",
                    },
                };
            }

            // Sort by most recently updated
            return tasks.OrderByDescending(t => t.UpdatedAt).ToList();
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"Fatal error retrieving Copilot tasks: {ex.Message}");
            _log.Error($"  Exception type: {ex.GetType().FullName}");
            _log.Error($"  Stack trace: {ex.StackTrace}");
            return new[]
            {
                new CopilotTask
                {
                    Id = "error-task",
                    Title = "Error Loading Tasks",
                    Description = $"An error occurred while loading Copilot tasks:\n\n{ex.Message}\n\nCheck the extension logs for more details.",
                    Status = CopilotTaskStatus.Failed,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Repository = "error",
                    Url = "https://github.com",
                    Agent = "Error Handler",
                },
            };
        }
    }

    private static CopilotTask CreateTaskFromIssue(Octokit.Issue issue, string botAuthor)
    {
        var status = issue.State.Value switch
        {
            Octokit.ItemState.Open => CopilotTaskStatus.InProgress,
            Octokit.ItemState.Closed when issue.PullRequest?.Merged == true => CopilotTaskStatus.Completed,
            Octokit.ItemState.Closed => CopilotTaskStatus.Cancelled,
            _ => CopilotTaskStatus.InProgress,
        };

        var agentType = DetermineBotType(botAuthor);
        var repository = issue.Repository?.FullName ?? ExtractRepoFromUrl(issue.HtmlUrl);
        var branch = issue.PullRequest?.Head?.Ref;

        return new CopilotTask
        {
            Id = $"pr-{issue.Id}",
            Title = issue.Title ?? "Untitled Pull Request",
            Description = $"{issue.Body ?? "No description provided"}",
            Status = status,
            CreatedAt = issue.CreatedAt.DateTime,
            UpdatedAt = issue.UpdatedAt?.DateTime ?? issue.CreatedAt.DateTime,
            Repository = repository,
            Branch = branch,
            Url = issue.HtmlUrl,
            Agent = agentType,
        };
    }

    private static CopilotTask CreateTaskFromPullRequest(Octokit.PullRequest pr, string repositoryFullName, string botAuthor)
    {
        var status = pr.State.Value switch
        {
            Octokit.ItemState.Open => CopilotTaskStatus.InProgress,
            Octokit.ItemState.Closed when pr.Merged => CopilotTaskStatus.Completed,
            Octokit.ItemState.Closed => CopilotTaskStatus.Cancelled,
            _ => CopilotTaskStatus.InProgress,
        };

        var agentType = DetermineBotType(botAuthor);

        return new CopilotTask
        {
            Id = $"pr-{pr.Id}",
            Title = pr.Title ?? "Untitled Pull Request",
            Description = $"{pr.Body ?? "No description provided"}",
            Status = status,
            CreatedAt = pr.CreatedAt.DateTime,
            UpdatedAt = pr.UpdatedAt.DateTime,
            Repository = repositoryFullName,
            Branch = pr.Head?.Ref,
            Url = pr.HtmlUrl,
            Agent = agentType,
        };
    }

    private static string DetermineBotType(string botName)
    {
        var lowerName = botName.ToLowerInvariant();

        if (lowerName.Contains("copilot-workspace", StringComparison.Ordinal))
        {
            return "GitHub Copilot Workspace";
        }

        if (lowerName.Contains("github-copilot", StringComparison.Ordinal))
        {
            return "GitHub Copilot";
        }

        if (lowerName.Contains("copilot", StringComparison.Ordinal))
        {
            return "GitHub Copilot";
        }

        return $"Bot ({botName})";
    }

    private static string ExtractRepoFromUrl(string htmlUrl)
    {
        try
        {
            var uri = new Uri(htmlUrl);
            var segments = uri.AbsolutePath.Trim('/').Split('/');
            if (segments.Length >= 2)
            {
                return $"{segments[0]}/{segments[1]}";
            }
        }
        catch
        {
            // Ignore parsing errors
        }

        return "Unknown Repository";
    }

    private static string CreateNoResultsMessage(string username)
    {
        return $"""
            No pull requests authored by GitHub Copilot were found.

            **User:** {username}
            **Search time:** {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC

            ## Why no tasks?

            1. **No Copilot PRs exist yet**
               - You haven't created any PRs using GitHub Copilot Workspace
               - Copilot bots haven't created PRs in repositories you can access

            2. **Repository access**
               - PRs must be in public repositories or private repos you have access to
               - GitHub search only returns results you have permission to see

            3. **GitHub API limitations**
               - Search API has rate limits
               - Some results may be filtered by GitHub

            ## How to create Copilot tasks:

            ### Use GitHub Copilot Workspace

            1. Visit **https://copilot-workspace.githubnext.com/**
            2. Sign in with your GitHub account
            3. Select a repository you own or have write access to
            4. Describe the changes you want (example: "Add a dark mode toggle")
            5. Review Copilot's proposed changes
            6. Click "Create Pull Request"
            7. Come back here and refresh to see your task!

            ### Example prompts to try:
            - "Add input validation to the login form"
            - "Fix the bug where dates display incorrectly"
            - "Add unit tests for the UserService class"
            - "Refactor the API endpoints to use async/await"

            ## Troubleshooting:

            **Still not seeing tasks?**

            1. **Check repository access**
               - Go to https://github.com/{username}?tab=repositories
               - Verify you have repositories you can create PRs in

            2. **Verify Copilot access**
               - Visit https://github.com/settings/copilot
               - Ensure Copilot is enabled for your account

            3. **Check GitHub token permissions**
               - Visit https://github.com/settings/tokens
               - Ensure your token has `repo` scope

            4. **Try a manual search**
               - Go to https://github.com/search
               - Search for: `is:pr author:copilot-workspace[bot]`
               - See if any results appear

            5. **Check extension logs**
               - Location: `%localappdata%\\Packages\\Microsoft.CmdPalGitHubExtension_*\\TempState`
               - Look for error messages or API responses

            ## What counts as a Copilot task?

            This feature shows pull requests created by:
            - **copilot-workspace[bot]** - GitHub Copilot Workspace
            - **github-copilot[bot]** - GitHub Copilot
            - **copilot[bot]** - Generic Copilot bot

            Regular PRs you create manually will NOT appear here.

            ---

            **Need help?** Visit https://github.com/microsoft/CmdPalGitHubExtension/issues
            """;
    }

    private async Task<string> SimulateCopilotResponse(string userMessage)
    {
        await Task.Delay(1000);
        var lowerMessage = userMessage.ToLowerInvariant();

        if (lowerMessage.Contains("issue") || lowerMessage.Contains("bug"))
        {
            return $"""
                Based on your query about "{userMessage}", here are some suggestions:

                **For GitHub Issues:**
                - Use descriptive titles that clearly state the problem
                - Include steps to reproduce the issue
                - Add relevant labels for categorization
                - Link to related pull requests or issues

                **Best Practices:**
                - Check existing issues before creating new ones
                - Use issue templates if available
                - Include environment details (OS, browser, versions)
                - Add screenshots or logs when applicable
                """;
        }

        if (lowerMessage.Contains("pull request") || lowerMessage.Contains("pr"))
        {
            return $"""
                Regarding your pull request query "{userMessage}":

                **PR Best Practices:**
                - Write clear, descriptive commit messages
                - Keep PRs focused and small when possible
                - Include a detailed description of changes
                - Link to related issues using keywords like "fixes #123"

                **Review Process:**
                - Request reviews from relevant team members
                - Respond promptly to feedback
                - Use draft PRs for work-in-progress
                - Ensure CI/CD checks pass before requesting review
                """;
        }

        return $"""
            Thanks for your question: "{userMessage}"

            **GitHub Copilot can help you with:**
            - Code suggestions and completions
            - Debugging and troubleshooting
            - Best practices for GitHub workflows
            - Repository management tips
            - Pull request and issue guidance

            Feel free to ask more specific questions about any GitHub functionality!
            """;
    }
}
