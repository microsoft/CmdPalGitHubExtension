// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;

namespace GitHubExtension.Helpers;

public static class TemplateHelper
{
    public static string GetTemplatePath(string page)
    {
        return page switch
        {
            "AuthTemplate" => "Controls\\Templates\\AuthTemplate.json",
            "SaveSearch" => "Controls\\Templates\\SaveSearchTemplate.json",
            _ => throw new NotImplementedException($"Template for page '{page}' is not implemented."),
        };
    }

    public static string LoadTemplateJsonFromTemplateName(string templateName, Dictionary<string, string> substitutions)
    {
        var path = Path.Combine(AppContext.BaseDirectory, GetTemplatePath(templateName));
        var template = File.ReadAllText(path, Encoding.Default) ?? throw new FileNotFoundException(path);

        if (substitutions.Count > 0)
        {
            template = FillInTemplate(template, substitutions);
        }

        return template;
    }

    public static string FillInTemplate(string jsonTemplate, Dictionary<string, string> substitutions)
    {
        foreach (var substitution in substitutions)
        {
            jsonTemplate = jsonTemplate.Replace(substitution.Key, substitution.Value);
        }

        return jsonTemplate;
    }

    public static string EscapeStringLiteralForJson(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input ?? string.Empty;
        }

        return input.Replace("\"", "\\\"");
    }
}
