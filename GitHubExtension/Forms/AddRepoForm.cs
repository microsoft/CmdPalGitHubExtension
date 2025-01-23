// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Nodes;
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

            var repositoryName = ExtractRepositoryName(repositoryUrl);
            var userName = _githubClient.User.Current().Result.Login;

            var isMember = IsUserMemberOfRepository(userName, repositoryName).Result;
            if (!isMember)
            {
                throw new UnauthorizedAccessException("User is not a member of the repository");
            }

            var userRepositories = GetUserRepositories(userName, repositoryName).Result;

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

    private async Task<bool> IsUserMemberOfRepository(string userName, string repositoryName)
    {
        try
        {
            var membership = await _githubClient.Repository.Collaborator.IsCollaborator(repositoryName, userName);
            return membership;
        }
        catch (NotFoundException)
        {
            return false;
        }
    }

    private async Task<IReadOnlyList<Repository>> GetUserRepositories(string userName, string repositoryName)
    {
        var userRepositories = new List<Repository>();
        var page = 1;
        const int pageSize = 100; // Adjust the page size as needed

        while (true)
        {
            var repositories = await _githubClient.Repository.GetAllForUser(userName, new ApiOptions
            {
                PageCount = 1,
                PageSize = pageSize,
                StartPage = page,
            });

            if (repositories.Count == 0)
            {
                break;
            }

            foreach (var repo in repositories)
            {
                try
                {
                    var isCollaborator = await _githubClient.Repository.Collaborator.IsCollaborator(repo.Id, userName);
                    if (isCollaborator)
                    {
                        userRepositories.Add(repo);
                    }
                }
                catch (Exception ex)
                {
                    Log.Information($"Repository {repo.Name} not added: {ex.Message}");
                }
            }

            page++;
        }

        if (userRepositories.Count == 0)
        {
            throw new InvalidOperationException("No repositories found for the user. See logs for more information");
        }

        return userRepositories;
    }

    private string ExtractRepositoryName(string repositoryUrl)
    {
        var uri = new Uri(repositoryUrl);
        var segments = uri.Segments;
        return segments.Length > 1 ? segments[1].TrimEnd('/') : string.Empty;
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
