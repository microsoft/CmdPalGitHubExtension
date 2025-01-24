// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using System.Text.Json.Nodes;
using GitHubExtension.Client;
using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Octokit;
using Serilog;
using Windows.Foundation;

namespace GitHubExtension.Forms;

internal sealed partial class AddRepoForm : Form
{
    internal event TypedEventHandler<object, object?>? RepositoryAdded;

    private readonly GitHubClient _githubClient;

    public AddRepoForm()
    {
        var developerIdProvider = DeveloperIdProvider.GetInstance();
        var developerId = developerIdProvider.GetLoggedInDeveloperIdsInternal().FirstOrDefault();
        if (developerId == null)
        {
            throw new InvalidOperationException("No logged-in developer ID found.");
        }

        _githubClient = developerId.GitHubClient;
    }

    public override ICommandResult SubmitForm(string payload)
    {
        try
        {
            var formInput = JsonNode.Parse(payload);
            if (formInput == null)
            {
                return CommandResult.GoHome();
            }

            Console.WriteLine($"Repository URL: {payload}"); // Debugging statement

            var repositoryUrl = formInput["repositoryUrl"]?.ToString();
            if (string.IsNullOrEmpty(repositoryUrl))
            {
                return CommandResult.GoHome();
            }

            var repositoryName = Validation.ParseRepositoryFromGitHubURL(repositoryUrl);
            var ownerName = Validation.ParseOwnerFromGitHubURL(repositoryUrl);
            var userName = _githubClient.User.Current().Result.Login;

            var isMember = IsUserMemberOfRepository(ownerName, repositoryName, userName).Result;
            var isContributor = IsUserContributorOfRepository(ownerName, repositoryName, userName).Result;

            if (!isMember && !isContributor)
            {
                throw new UnauthorizedAccessException("User is not a member of the repository");
            }

            var repoHelper = GitHubRepositoryHelper.Instance;
            var repositories = repoHelper.GetUserRepositories();
            repoHelper.AddRepository(ownerName, repositoryName);

            // Process the userRepositories as needed
            RepositoryAdded?.Invoke(this, null);
            return CommandResult.KeepOpen();
        }
        catch (Exception ex)
        {
            RepositoryAdded?.Invoke(this, ex);
            return CommandResult.KeepOpen();
        }
    }

    private async Task<bool> IsUserMemberOfRepository(string ownerName, string repositoryName, string userName)
    {
        try
        {
            var collaborators = await _githubClient.Issue.Assignee.GetAllForRepository(ownerName, repositoryName);
            if (collaborators.Count == 0)
            {
                return false;
            }
            else
            {
                return collaborators.Any(collaborator => collaborator.Login == userName);
            }
        }
        catch (NotFoundException)
        {
            return false;
        }
    }

    private async Task<bool> IsUserContributorOfRepository(string ownerName, string repositoryName, string userName)
    {
        try
        {
            var commits = await _githubClient.Repository.Commit.GetAll(ownerName, repositoryName, new CommitRequest { Author = userName });
            var issueSearchRequest = new SearchIssuesRequest(repositoryName)
            {
                Author = userName,
                Type = IssueTypeQualifier.PullRequest,
                SortField = IssueSearchSort.Created,
                Order = SortDirection.Descending,
            };

            var pullRequests = await _githubClient.Search.SearchIssues(issueSearchRequest);

            return (commits.Count > 0) || (pullRequests.TotalCount > 0);
        }
        catch (NotFoundException)
        {
            return false;
        }
    }

    public override string TemplateJson()
    {
        var gh_base64 = GitHubIcon.GetBase64Icon("logo");
        var template = $@"
        {{
            ""type"": ""AdaptiveCard"",
            ""version"": ""1.3"",
            ""body"": [
                {{
                    ""type"": ""Image"",
                    ""url"": ""data:image/png;base64,%GitHubLogo%"",
                    ""size"": ""large"",
                    ""horizontalAlignment"": ""center""
                }},
                {{
                    ""type"": ""Container"",
                    ""items"": [
                        {{
                            ""type"": ""TextBlock"",
                            ""text"": ""Repository URL"",
                            ""weight"": ""bolder"",
                            ""size"": ""medium""
                        }},
                        {{
                            ""type"": ""Input.Text"",
                            ""id"": ""repositoryUrl"",
                            ""placeholder"": ""Enter repository URL""
                        }}
                    ],
                    ""horizontalAlignment"": ""left""
                }}
            ],
            ""actions"": [
                {{
                    ""type"": ""Action.Submit"",
                    ""title"": ""Submit"",
                    ""data"": {{
                        ""repositoryUrl"": ""{{repositoryUrl.value}}""
                    }}
                }}
            ]
        }}";
        template = template.Replace("%GitHubLogo%", gh_base64);
        return template;
    }

    private sealed class Payload
    {
        public string RepositoryUrl { get; set; } = string.Empty;
    }
}
