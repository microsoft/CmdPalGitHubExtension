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

public sealed partial class TriggerWorkflowForm : FormContent, IGitHubForm
{
    private readonly IWorkflowRunsDataManager _dataManager;
    private readonly IResources _resources;
    private readonly string _initialRepository;

    public event EventHandler<bool>? LoadingStateChanged;

    public event EventHandler<FormSubmitEventArgs>? FormSubmitted;

    public TriggerWorkflowForm(IWorkflowRunsDataManager dataManager, IResources resources, string initialRepository = "")
    {
        _dataManager = dataManager;
        _resources = resources;
        _initialRepository = initialRepository;
    }

    public Dictionary<string, string> TemplateSubstitutions => new()
    {
        { "{{TriggerWorkflowFormTitle}}", JsonSerializer.Serialize(_resources.GetResource("Forms_TriggerWorkflow_Title")) },
        { "{{RepositoryLabel}}", JsonSerializer.Serialize(_resources.GetResource("Forms_Create_RepositoryLabel")) },
        { "{{RepositoryPlaceholder}}", JsonSerializer.Serialize(_resources.GetResource("Forms_Create_RepositoryPlaceholder")) },
        { "{{RepositoryErrorMessage}}", JsonSerializer.Serialize(_resources.GetResource("Forms_Create_RepositoryError")) },
        { "{{RepositoryValue}}", JsonSerializer.Serialize(_initialRepository) },
        { "{{WorkflowFileLabel}}", JsonSerializer.Serialize(_resources.GetResource("Forms_TriggerWorkflow_WorkflowFileLabel")) },
        { "{{WorkflowFilePlaceholder}}", JsonSerializer.Serialize(_resources.GetResource("Forms_TriggerWorkflow_WorkflowFilePlaceholder")) },
        { "{{WorkflowFileErrorMessage}}", JsonSerializer.Serialize(_resources.GetResource("Forms_TriggerWorkflow_WorkflowFileError")) },
        { "{{RefLabel}}", JsonSerializer.Serialize(_resources.GetResource("Forms_TriggerWorkflow_RefLabel")) },
        { "{{RefPlaceholder}}", JsonSerializer.Serialize(_resources.GetResource("Forms_TriggerWorkflow_RefPlaceholder")) },
        { "{{RefErrorMessage}}", JsonSerializer.Serialize(_resources.GetResource("Forms_TriggerWorkflow_RefError")) },
        { "{{RefValue}}", JsonSerializer.Serialize("main") },
        { "{{TriggerWorkflowActionTitle}}", JsonSerializer.Serialize(_resources.GetResource("Forms_TriggerWorkflow_Action")) },
    };

    public override string TemplateJson => TemplateHelper.LoadTemplateJsonFromTemplateName("TriggerWorkflow", TemplateSubstitutions);

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
            var workflowFile = payload["WorkflowFile"]?.ToString() ?? string.Empty;
            var gitRef = payload["Ref"]?.ToString() ?? string.Empty;

            await _dataManager.TriggerWorkflowAsync(repository, workflowFile, gitRef);

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
