// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.Helpers;

internal static class GitHubHelper
{
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
            "AuthTemplate" => "Controls\\Templates\\AuthTemplate.json",
            "SaveSearch" => "Controls\\Templates\\SaveSearchTemplate.json",
            _ => throw new NotImplementedException(),
        };
    }

    public static Dictionary<string, string> ParseOwnerAndRepoFromSearchString(string searchString)
    {
        const string repoPrefix = "repo:";
        var parts = searchString.Split(' ');

        foreach (var part in parts)
        {
            if (part.StartsWith(repoPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var repoInfo = part.Substring(repoPrefix.Length).Split('/');
                if (repoInfo.Length == 2)
                {
                    return new Dictionary<string, string>
                    {
                        { "owner", repoInfo[0] },
                        { "repo", repoInfo[1] },
                    };
                }
            }
        }

        return new Dictionary<string, string>();
    }
}
