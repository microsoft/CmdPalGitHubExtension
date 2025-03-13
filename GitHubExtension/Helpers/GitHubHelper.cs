// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.Helpers;

internal static class GitHubHelper
{
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
