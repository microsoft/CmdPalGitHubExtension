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

public sealed partial class CloneRepositoryForm : FormContent, IGitHubForm
{
    private readonly IRepositoryCloneManager _cloneManager;
    private readonly ICloneSettingsStore _settingsStore;
    private readonly IResources _resources;
    private readonly string _initialRepository;

    public event EventHandler<bool>? LoadingStateChanged;

    public event EventHandler<FormSubmitEventArgs>? FormSubmitted;

    public CloneRepositoryForm(IRepositoryCloneManager cloneManager, ICloneSettingsStore settingsStore, IResources resources, string initialRepository = "")
    {
        _cloneManager = cloneManager;
        _settingsStore = settingsStore;
        _resources = resources;
        _initialRepository = initialRepository;
    }

    public Dictionary<string, string> TemplateSubstitutions => new()
    {
        { "{{CloneRepositoryFormTitle}}", JsonSerializer.Serialize(_resources.GetResource("Forms_Clone_Title")) },
        { "{{RepositoryLabel}}", JsonSerializer.Serialize(_resources.GetResource("Forms_Clone_RepositoryLabel")) },
        { "{{RepositoryPlaceholder}}", JsonSerializer.Serialize(_resources.GetResource("Forms_Clone_RepositoryPlaceholder")) },
        { "{{RepositoryErrorMessage}}", JsonSerializer.Serialize(_resources.GetResource("Forms_Clone_RepositoryError")) },
        { "{{RepositoryValue}}", JsonSerializer.Serialize(_initialRepository) },
        { "{{TargetDirectoryLabel}}", JsonSerializer.Serialize(_resources.GetResource("Forms_Clone_TargetDirectoryLabel")) },
        { "{{TargetDirectoryPlaceholder}}", JsonSerializer.Serialize(_settingsStore.GetDefaultCloneBaseDirectory()) },
        { "{{CloneRepositoryActionTitle}}", JsonSerializer.Serialize(_resources.GetResource("Forms_Clone_Action")) },
    };

    public override string TemplateJson => TemplateHelper.LoadTemplateJsonFromTemplateName("CloneRepository", TemplateSubstitutions);

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
            var targetDirectory = payload["TargetDirectory"]?.ToString() ?? string.Empty;

            await _cloneManager.CloneRepositoryAsync(repository, targetDirectory);

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
