// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Foundation;

namespace GitHubExtension.Forms.Templates;

public abstract partial class GitHubForm : FormContent
{
    private readonly Dictionary<string, string> templateSubstitutions = new();

    public virtual Dictionary<string, string> TemplateSubstitutions => templateSubstitutions;

    public abstract ICommandResult DefaultSubmitFormCommand { get; }

    public event TypedEventHandler<object, bool>? LoadingStateChanged;

    public event TypedEventHandler<object, FormSubmitEventArgs>? FormSubmitted;

    public override ICommandResult SubmitForm(string inputs)
    {
        LoadingStateChanged?.Invoke(this, true);
        Task.Run(() => HandleSubmit(inputs));
        return DefaultSubmitFormCommand;
    }

    public abstract void HandleSubmit(string payload);

    public virtual string LoadTemplateJsonFromFile(string templateName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, GitHubHelper.GetTemplatePath(templateName));
        var template = File.ReadAllText(path, Encoding.Default) ?? throw new FileNotFoundException(path);

        if (TemplateSubstitutions.Count > 0)
        {
            template = FillInTemplate(template, TemplateSubstitutions);
        }

        return template;
    }

    public virtual string FillInTemplate(string jsonTemplate, Dictionary<string, string> substitutions)
    {
        foreach (var substitution in substitutions)
        {
            jsonTemplate = jsonTemplate.Replace(substitution.Key, substitution.Value);
        }

        return jsonTemplate;
    }

    protected void RaiseLoadingStateChanged(bool isLoading)
    {
        LoadingStateChanged?.Invoke(this, isLoading);
    }

    protected void RaiseFormSubmitted(FormSubmitEventArgs args)
    {
        FormSubmitted?.Invoke(this, args);
    }
}
