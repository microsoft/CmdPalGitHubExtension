// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.Helpers;

internal sealed class GitHubHelper
{
    internal static string StateJsonPath()
    {
        // Get the path to our exe
        var path = System.Reflection.Assembly.GetExecutingAssembly().Location;

        // Get the directory of the exe
        var directory = Path.GetDirectoryName(path) ?? string.Empty;

        // now, the state is just next to the exe
        return Path.Combine(directory, "state.json");
    }

    internal static string StateTemplateJsonPath(string templateName)
    {
        // Get the path to our exe
        var path = System.Reflection.Assembly.GetExecutingAssembly().Location;

        // Get the directory of the exe
        var directory = Path.GetDirectoryName(path) ?? string.Empty;

        var templatePath = Path.Combine(directory, "Templates");

        // now, the state is just next to the exe
        return Path.Combine(templatePath, templateName);
    }

    public static string GetTemplatePath(string page)
    {
        return page switch
        {
            "SignIn" => "Templates\\AuthTemplate.json",
            "SaveSearchSurvey" => "Templates\\SaveSearchSurveyTemplate.json",
            "SaveSearchSurveyData" => "Templates\\SaveSearchSurveyData.json",
            "SaveSearch" => "Templates\\SaveSearchTemplate.json",
            _ => throw new NotImplementedException(),
        };
    }
}
