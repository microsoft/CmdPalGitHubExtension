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

internal sealed partial class AddRepoForm : GitHubForm
{
    public override Dictionary<string, string> TemplateSubstitutions => new()
    {
        { "{{GitHubIcon}}", $"data:image/png;base64,{GitHubIcon.GetBase64Icon("logo")}" },
        { "{{AddRepoTitle}}", "Add Repository" },
        { "{{RepoURLPlaceholder}}", "Enter repository URL" },
        { "{{AddRepoButtonTitle}}", "Add" },
    };

    public override ICommandResult DefaultSubmitFormCommand => CommandResult.KeepOpen();

    internal event TypedEventHandler<object, object?>? RepositoryAdded;

    public override void HandleSubmit(string payload)
    {
        try
        {
            var repoInfo = ParseSubmission(payload);

            if (repoInfo.Length == 1)
            {
                RepositoryAdded?.Invoke(this, new InvalidOperationException(repoInfo[0]));
                return;
            }

            var ownerName = repoInfo[0];
            var repositoryName = repoInfo[1];
            var repoHelper = GitHubRepositoryHelper.Instance;

            Task.Run(() => repoHelper.AddRepository(ownerName, repositoryName)).Wait();

            RepositoryAdded?.Invoke(this, null);
            RaiseLoadingStateChanged(false);
            RaiseFormSubmitted(new FormSubmitEventArgs(true, null));
        }
        catch (Exception ex)
        {
            RepositoryAdded?.Invoke(this, ex);
            RaiseLoadingStateChanged(false);
            RaiseFormSubmitted(new FormSubmitEventArgs(false, ex));
        }
    }

    private string[] ParseSubmission(string payload)
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

    public override string TemplateJson() => LoadTemplateJsonFromFile("AddRepo");
}
