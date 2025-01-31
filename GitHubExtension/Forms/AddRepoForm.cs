// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Nodes;
using GitHubExtension.Client;
using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using GitHubExtension.Pages;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Octokit;
using Windows.Foundation;

namespace GitHubExtension.Forms;

internal sealed partial class AddRepoForm : Form
{
    internal event TypedEventHandler<object, object?>? RepositoryAdded;

    private readonly GitHubClient _githubClient;

    private readonly AddRepoPage _addRepoPage;

    public AddRepoForm(AddRepoPage page)
    {
        var developerIdProvider = DeveloperIdProvider.GetInstance();
        var developerId = developerIdProvider.GetLoggedInDeveloperIdsInternal().FirstOrDefault();
        if (developerId == null)
        {
            throw new InvalidOperationException("No logged-in developer ID found.");
        }

        _githubClient = developerId.GitHubClient;
        _addRepoPage = page;
    }

    public override ICommandResult SubmitForm(string payload)
    {
        try
        {
            _addRepoPage.IsLoading = true;

            Task.Run(async () => await HandleSubmit(payload));

            return CommandResult.KeepOpen();
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage(new LogMessage() { Message = $"Error in SubmitForm: {ex.Message}" });
            return CommandResult.GoHome();
        }
    }

    private async Task HandleSubmit(string payload)
    {
        var repoInfo = await ProcessSubmission(payload);

        if (repoInfo.Length == 1)
        {
            RepositoryAdded?.Invoke(this, new InvalidOperationException(repoInfo[0]));
            return;
        }

        var ownerName = repoInfo[0];
        var repositoryName = repoInfo[1];

        ExtensionHost.LogMessage(new LogMessage() { Message = $"IsMemberOrContributor {IsMemberOrContributor(ownerName, repositoryName)}..." });

        var repoHelper = GitHubRepositoryHelper.Instance;
        var repositories = repoHelper.GetUserRepositories();
        repoHelper.AddRepository(ownerName, repositoryName);

        RepositoryAdded?.Invoke(this, null);
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    private async Task<string[]> ProcessSubmission(string payload)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        try
        {
            var formInput = JsonNode.Parse(payload);
            if (formInput == null)
            {
                throw new InvalidOperationException("No input found");
            }

            var repositoryUrl = formInput["repositoryUrl"]?.ToString();
            if (string.IsNullOrEmpty(repositoryUrl))
            {
                throw new InvalidOperationException("No repository URL found");
            }

            var repositoryName = Validation.ParseRepositoryFromGitHubURL(repositoryUrl);
            var ownerName = Validation.ParseOwnerFromGitHubURL(repositoryUrl);
            return new[] { ownerName, repositoryName };
        }
        catch (Exception ex)
        {
            RepositoryAdded?.Invoke(this, ex);
            return new[] { ex.Message };
        }
    }

    private bool IsMemberOrContributor(string ownerName, string repositoryName)
    {
        var userName = _githubClient.User.Current().Result.Login;

        var isMember = IsUserMemberOfRepository(ownerName, repositoryName, userName).Result;
        var isContributor = IsUserContributorOfRepository(ownerName, repositoryName, userName).Result;

        return isMember || isContributor;
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
