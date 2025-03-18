// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.Helpers;

public static class GitStringHelper
{
    public static string SwapGitColonsForForwardSlashes(string gitBranch)
    {
        if (gitBranch == null)
        {
            return string.Empty;
        }

        var parts = gitBranch.Split(':');
        if (parts.Length > 1)
        {
            return string.Join("/", parts);
        }

        return gitBranch;
    }
}
