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

public sealed partial class AddCommentForm : FormContent, IGitHubForm
{
    private readonly IGitHubCreateManager _createManager;
    private readonly IResources _resources;
    private readonly IIssue _issue;

    public event EventHandler<bool>? LoadingStateChanged;

    public event EventHandler<FormSubmitEventArgs>? FormSubmitted;

    public AddCommentForm(IIssue issue, IGitHubCreateManager createManager, IResources resources)
    {
        _issue = issue;
        _createManager = createManager;
        _resources = resources;
    }

    public Dictionary<string, string> TemplateSubstitutions => new()
    {
        { "{{AddCommentFormTitle}}", JsonSerializer.Serialize(_resources.GetResource("Forms_AddComment_Title")) },
        { "{{AddCommentSubtitle}}", JsonSerializer.Serialize(_issue.Title) },
        { "{{BodyLabel}}", JsonSerializer.Serialize(_resources.GetResource("Forms_AddComment_BodyLabel")) },
        { "{{BodyPlaceholder}}", JsonSerializer.Serialize(_resources.GetResource("Forms_AddComment_BodyPlaceholder")) },
        { "{{BodyErrorMessage}}", JsonSerializer.Serialize(_resources.GetResource("Forms_AddComment_BodyError")) },
        { "{{AddCommentActionTitle}}", JsonSerializer.Serialize(_resources.GetResource("Forms_AddComment_Action")) },
    };

    public override string TemplateJson => TemplateHelper.LoadTemplateJsonFromTemplateName("AddComment", TemplateSubstitutions);

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
            var body = payload["Body"]?.ToString() ?? string.Empty;

            await _createManager.AddCommentAsync(_issue, body);

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
