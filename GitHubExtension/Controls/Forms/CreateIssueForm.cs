// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using System.Text.Json.Nodes;
using GitHubExtension.DataManager;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Forms;

public sealed partial class CreateIssueForm : FormContent, IGitHubForm
{
    private readonly IGitHubCreateManager _createManager;
    private readonly IResources _resources;
    private readonly string _initialRepository;

    public event EventHandler<bool>? LoadingStateChanged;

    public event EventHandler<FormSubmitEventArgs>? FormSubmitted;

    public CreateIssueForm(IGitHubCreateManager createManager, IResources resources, string initialRepository = "")
    {
        _createManager = createManager;
        _resources = resources;
        _initialRepository = initialRepository;
    }

    public Dictionary<string, string> TemplateSubstitutions => new()
    {
        { "{{CreateIssueFormTitle}}", JsonSerializer.Serialize(_resources.GetResource("Forms_CreateIssue_Title")) },
        { "{{RepositoryLabel}}", JsonSerializer.Serialize(_resources.GetResource("Forms_Create_RepositoryLabel")) },
        { "{{RepositoryPlaceholder}}", JsonSerializer.Serialize(_resources.GetResource("Forms_Create_RepositoryPlaceholder")) },
        { "{{RepositoryErrorMessage}}", JsonSerializer.Serialize(_resources.GetResource("Forms_Create_RepositoryError")) },
        { "{{RepositoryValue}}", JsonSerializer.Serialize(_initialRepository) },
        { "{{TitleLabel}}", JsonSerializer.Serialize(_resources.GetResource("Forms_Create_TitleLabel")) },
        { "{{TitlePlaceholder}}", JsonSerializer.Serialize(_resources.GetResource("Forms_Create_TitlePlaceholder")) },
        { "{{TitleErrorMessage}}", JsonSerializer.Serialize(_resources.GetResource("Forms_Create_TitleError")) },
        { "{{BodyLabel}}", JsonSerializer.Serialize(_resources.GetResource("Forms_Create_BodyLabel")) },
        { "{{BodyPlaceholder}}", JsonSerializer.Serialize(_resources.GetResource("Forms_Create_BodyPlaceholder")) },
        { "{{CreateIssueActionTitle}}", JsonSerializer.Serialize(_resources.GetResource("Forms_CreateIssue_Action")) },
    };

    public override string TemplateJson => TemplateHelper.LoadTemplateJsonFromTemplateName("CreateIssue", TemplateSubstitutions);

    public override ICommandResult SubmitForm(string? inputs, string data)
    {
        LoadingStateChanged?.Invoke(this, true);
        _ = SubmitInternalAsync(inputs);
        return CommandResult.KeepOpen();
    }

    private async Task SubmitInternalAsync(string? inputs)
    {
        try
        {
            var payload = JsonNode.Parse(inputs ?? string.Empty) ?? throw new InvalidOperationException("No input provided.");
            var repository = payload["Repository"]?.ToString() ?? string.Empty;
            var title = payload["Title"]?.ToString() ?? string.Empty;
            var body = payload["Body"]?.ToString() ?? string.Empty;

            await _createManager.CreateIssueAsync(repository, title, body);

            LoadingStateChanged?.Invoke(this, false);
            FormSubmitted?.Invoke(this, new FormSubmitEventArgs(true, null));
        }
        catch (Exception ex)
        {
            LoadingStateChanged?.Invoke(this, false);
            FormSubmitted?.Invoke(this, new FormSubmitEventArgs(false, ex));
        }
    }
}
