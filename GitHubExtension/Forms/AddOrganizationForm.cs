// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Nodes;
using GitHubExtension.DeveloperId;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Octokit;
using Serilog;
using Windows.Foundation;

namespace GitHubExtension.Forms;

internal sealed partial class AddOrganizationForm : Form
{
    internal event TypedEventHandler<object, object?>? OrganizationAdded;

    private readonly GitHubClient _githubClient;

    public AddOrganizationForm()
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

            Console.WriteLine($"Organization URL: {payload}"); // Debugging statement

            var organizationUrl = formInput["organizationUrl"]?.ToString();
            if (string.IsNullOrEmpty(organizationUrl))
            {
                return CommandResult.GoHome();
            }

            var organizationName = ExtractOrganizationName(organizationUrl);
            var userName = _githubClient.User.Current().Result.Login;

            var isMember = IsUserMemberOfOrganization(userName, organizationName).Result;
            if (!isMember)
            {
                throw new UnauthorizedAccessException("User is not a member of the organization");
            }

            var userRepositories = GetUserRepositoriesInOrganization(userName, organizationName).Result;

            // Process the userRepositories as needed
            OrganizationAdded?.Invoke(this, null);
            return CommandResult.KeepOpen();
        }
        catch (Exception ex)
        {
            OrganizationAdded?.Invoke(this, ex);
            return CommandResult.KeepOpen();
        }
    }

    private async Task<bool> IsUserMemberOfOrganization(string userName, string organizationName)
    {
        try
        {
            var membership = await _githubClient.Organization.Member.CheckMember(organizationName, userName);
            return membership;
        }
        catch (NotFoundException)
        {
            return false;
        }
    }

    private async Task<IReadOnlyList<Repository>> GetUserRepositoriesInOrganization(string userName, string organizationName)
    {
        // Currently only gets public repos from organization
        var repositories = await _githubClient.Repository.GetAllForOrg(organizationName);
        var userRepositories = new List<Repository>();

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

        if (userRepositories.Count == 0)
        {
            throw new InvalidOperationException("No repositories found for the user in the organization. See logs for more information");
        }

        return userRepositories;
    }

    private string ExtractOrganizationName(string organizationUrl)
    {
        var uri = new Uri(organizationUrl);
        var segments = uri.Segments;
        return segments.Length > 1 ? segments[1].TrimEnd('/') : string.Empty;
    }

    public override string TemplateJson()
    {
        return @"
        {
            ""type"": ""AdaptiveCard"",
            ""version"": ""1.3"",
            ""body"": [
                {
                    ""type"": ""Image"",
                    ""url"": ""https://github.githubassets.com/images/modules/logos_page/GitHub-Mark.png"",
                    ""horizontalAlignment"": ""center""
                },
                {
                    ""type"": ""Container"",
                    ""items"": [
                        {
                            ""type"": ""TextBlock"",
                            ""text"": ""Organization URL"",
                            ""weight"": ""bolder"",
                            ""size"": ""medium""
                        },
                        {
                            ""type"": ""Input.Text"",
                            ""id"": ""organizationUrl"",
                            ""placeholder"": ""Enter organization URL""
                        }
                    ],
                    ""horizontalAlignment"": ""left""
                }
            ],
            ""actions"": [
                {
                    ""type"": ""Action.Submit"",
                    ""title"": ""Submit"",
                    ""data"": {
                        ""organizationUrl"": ""{{organizationUrl.value}}""
                    }
                }
            ]
        }";
    }

    private sealed class Payload
    {
        public string OrganizationUrl { get; set; } = string.Empty;
    }
}
