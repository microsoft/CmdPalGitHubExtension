// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Foundation;

namespace GitHubExtension;

internal abstract partial class GitHubForm : Form
{
    private readonly Dictionary<string, string> templateSubstitutions = new();

    public virtual Dictionary<string, string> TemplateSubstitutions => templateSubstitutions;

    public override ICommandResult SubmitForm(string payload) => throw new NotImplementedException();

    public static event TypedEventHandler<object, bool>? LoadingStateChanged;

    public static event TypedEventHandler<object, FormSubmitEventArgs>? FormSubmitted;

    public override string DataJson()
    {
        return Data;
    }

    public override string StateJson()
    {
        return State;
    }

    public override string TemplateJson()
    {
        return LoadTemplateJsonFromFile(string.Empty);
    }

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

    public virtual string FillInTemplate(string template, Dictionary<string, string> substitutions)
    {
        foreach (var substitution in substitutions)
        {
            template = template.Replace(substitution.Key, substitution.Value);
        }

        return template;
    }
}
