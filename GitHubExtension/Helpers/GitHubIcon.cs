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
                { "pullRequest", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "pulls.png") },
                { "release", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "releases.png") },
                { "logo", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "gh_logo.jpg") },
            };
    }

    public static Dictionary<string, string> IconDictionary { get; private set; }
}
