// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Helpers;

public static class GitHubIcon
{
    public static string LogoWithBackplatePath { get; } = Path.Combine(AppContext.BaseDirectory, "Assets", "gh_logo.png");

    static GitHubIcon()
    {
        IconDictionary = new Dictionary<string, IconInfo>
            {
                { "issue", IconHelpers.FromRelativePath("Assets\\issues.svg") },
                { "pr", IconHelpers.FromRelativePath("Assets\\pulls.svg") },
                { "release", IconHelpers.FromRelativePath("Assets\\releases.svg") },
                { "logo", IconHelpers.FromRelativePaths("Assets\\github.light.svg", "Assets\\github.dark.svg") },
                { "Issues", IconHelpers.FromRelativePath("Assets\\issues.svg") },
                { "PullRequests", IconHelpers.FromRelativePath("Assets\\pulls.svg") },
                { "IssuesAndPullRequests", IconHelpers.FromRelativePaths("Assets\\github.light.svg", "Assets\\github.dark.svg") },
                { "Repositories", IconHelpers.FromRelativePaths("Assets\\github.light.svg", "Assets\\github.dark.svg") },
                { "Search", new IconInfo("\ue721") },
            };
    }

    public static Dictionary<string, IconInfo> IconDictionary { get; private set; }

    public static string GetBase64Icon(string iconPath)
    {
        if (!string.IsNullOrEmpty(iconPath))
        {
            var bytes = File.ReadAllBytes(iconPath);
            return Convert.ToBase64String(bytes);
        }

        return string.Empty;
    }
}
