// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.Helpers;

public static class GitHubIcon
{
    static GitHubIcon()
    {
        IconDictionary = new Dictionary<string, string>
            {
                { "logo_dark", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "GitHubLogo_Dark.png") },
                { "logo_light", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "GitHubLogo_Light.png") },
                { "issue", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "issues.png") },
                { "pr", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "pulls.png") },
                { "release", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "releases.png") },
                { "logo", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "gh_logo.jpg") },
                { "Issues", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "issues.png") },
                { "PullRequests", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "pulls.png") },
                { "IssuesAndPullRequests", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "gh_logo.jpg") },
            };
    }

    public static Dictionary<string, string> IconDictionary { get; private set; }

    public static string GetBase64Icon(string iconKey)
    {
        if (IconDictionary.TryGetValue(iconKey, out var iconPath))
        {
            var bytes = File.ReadAllBytes(iconPath);
            return Convert.ToBase64String(bytes);
        }

        return string.Empty;
    }
}
