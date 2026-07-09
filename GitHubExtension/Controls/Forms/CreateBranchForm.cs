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

public sealed partial class CreateBranchForm : FormContent, IGitHubForm
{
    private readonly IGitHubCreateManager _createManager;
    private readonly IResources _resources;
    private readonly string _initialRepository;

    public event EventHandler<bool>? LoadingStateChanged;

    public event EventHandler<FormSubmitEventArgs>? FormSubmitted;

    public CreateBranchForm(IGitHubCreateManager createManager, IResources resources, string initialRepository = "")
    {
        _createManager = createManager;
        _resources = resources;
        _initialRepository = initialRepository;
    }

    public Dictionary<string, string> TemplateSubstitutions => new()
    {
        { "{{CreateBranchFormTitle}}", JsonSerializer.Serialize(_resources.GetResource("Forms_CreateBranch_Title")) },
        { "{{RepositoryLabel}}", JsonSerializer.Serialize(_resources.GetResource("Forms_Create_RepositoryLabel")) },
        { "{{RepositoryPlaceholder}}", JsonSerializer.Serialize(_resources.GetResource("Forms_Create_RepositoryPlaceholder")) },
        { "{{RepositoryErrorMessage}}", JsonSerializer.Serialize(_resources.GetResource("Forms_Create_RepositoryError")) },
        { "{{RepositoryValue}}", JsonSerializer.Serialize(_initialRepository) },
        { "{{NewBranchLabel}}", JsonSerializer.Serialize(_resources.GetResource("Forms_CreateBranch_NewBranchLabel")) },
        { "{{NewBranchPlaceholder}}", JsonSerializer.Serialize(_resources.GetResource("Forms_CreateBranch_NewBranchPlaceholder")) },
        { "{{NewBranchErrorMessage}}", JsonSerializer.Serialize(_resources.GetResource("Forms_CreateBranch_NewBranchError")) },
        { "{{SourceBranchLabel}}", JsonSerializer.Serialize(_resources.GetResource("Forms_CreateBranch_SourceBranchLabel")) },
        { "{{SourceBranchPlaceholder}}", JsonSerializer.Serialize(_resources.GetResource("Forms_CreateBranch_SourceBranchPlaceholder")) },
        { "{{SourceBranchErrorMessage}}", JsonSerializer.Serialize(_resources.GetResource("Forms_CreateBranch_SourceBranchError")) },
        { "{{SourceBranchValue}}", JsonSerializer.Serialize("main") },
        { "{{CreateBranchActionTitle}}", JsonSerializer.Serialize(_resources.GetResource("Forms_CreateBranch_Action")) },
    };

    public override string TemplateJson => TemplateHelper.LoadTemplateJsonFromTemplateName("CreateBranch", TemplateSubstitutions);

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
            var newBranch = payload["NewBranch"]?.ToString() ?? string.Empty;
            var sourceBranch = payload["SourceBranch"]?.ToString() ?? string.Empty;

            await _createManager.CreateBranchAsync(repository, newBranch, sourceBranch);

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
