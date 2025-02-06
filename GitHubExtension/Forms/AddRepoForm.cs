// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Nodes;
using GitHubExtension.Client;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Foundation;

namespace GitHubExtension.Forms;

internal sealed partial class AddRepoForm : Form
{
    internal event TypedEventHandler<object, object?>? RepositoryAdded;

    internal event TypedEventHandler<object, bool>? LoadingStateChanged;

    public override ICommandResult SubmitForm(string payload)
    {
        try
        {
            LoadingStateChanged?.Invoke(this, true);

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
        try
        {
            var repoInfo = await ProcessSubmission(payload);

            if (repoInfo.Length == 1)
            {
                RepositoryAdded?.Invoke(this, new InvalidOperationException(repoInfo[0]));
                LoadingStateChanged?.Invoke(this, false);
                return;
            }

            var ownerName = repoInfo[0];
            var repositoryName = repoInfo[1];
            var repoHelper = GitHubRepositoryHelper.Instance;

            ExtensionHost.LogMessage(new LogMessage() { Message = $"IsMemberOrContributor {repoHelper.IsMemberOrContributor(ownerName, repositoryName)}..." });
            var repositories = repoHelper.GetUserRepositories();
            repoHelper.AddRepository(ownerName, repositoryName);

            RepositoryAdded?.Invoke(this, null);
            LoadingStateChanged?.Invoke(this, false);
        }
        catch (Exception ex)
        {
            RepositoryAdded?.Invoke(this, ex);
            LoadingStateChanged?.Invoke(this, false);
        }
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
