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
            if (!isMember)
            {
                throw new UnauthorizedAccessException("User is not a member of the repository");
            }

            var userRepositories = GetUserRepositories(ownerName, repositoryName).Result;

            // Save the repository information
            SaveRepositoryInformation(repositoryName, repositoryUrl);

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

    private void SaveRepositoryInformation(string repositoryName, string repositoryUrl)
    {
        var repoInfo = new { Name = repositoryName, Url = repositoryUrl };
        var json = JsonSerializer.Serialize(repoInfo);
        File.WriteAllText("repositoryInfo.json", json);
    }

    private async Task<bool> IsUserMemberOfRepository(string ownerName, string repositoryName, string userName)
    {
        try
        {
            // Get the repository by name to retrieve its ID
            var repository = await _githubClient.Repository.Get(ownerName, repositoryName);
            var membership = await _githubClient.Repository.Collaborator.IsCollaborator(repository.Id, userName);
            return membership;
        }
        catch (NotFoundException)
        {
            return false;
        }
    }

    private async Task<IReadOnlyList<Repository>> GetUserRepositories(string ownerName, string repositoryName)
    {
        var userRepositories = new List<Repository>();
        var page = 1;
        const int pageSize = 100; // Adjust the page size as needed

        while (true)
        {
            var repositories = await _githubClient.Repository.GetAllForUser(ownerName, new ApiOptions
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
                    var isCollaborator = await _githubClient.Repository.Collaborator.IsCollaborator(repo.Id, ownerName);
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
