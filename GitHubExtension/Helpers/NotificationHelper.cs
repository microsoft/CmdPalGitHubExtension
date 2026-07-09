// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.Helpers;

public static class NotificationHelper
{
    // Converts a notification subject API URL (e.g.
    // https://api.github.com/repos/owner/repo/issues/1) into the equivalent
    // browser URL (https://github.com/owner/repo/issues/1). When the subject
    // does not map cleanly to a browsable page, falls back to the repository URL.
    public static string GetHtmlUrl(string? subjectApiUrl, string? subjectType, string repositoryHtmlUrl)
    {
        if (string.IsNullOrEmpty(subjectApiUrl))
        {
            return repositoryHtmlUrl;
        }

        // Releases expose an internal id in the API URL that does not map to a
        // browsable tag URL, so link to the repository releases page instead.
        if (string.Equals(subjectType, "Release", StringComparison.OrdinalIgnoreCase))
        {
            return string.IsNullOrEmpty(repositoryHtmlUrl) ? subjectApiUrl : $"{repositoryHtmlUrl}/releases";
        }

        var htmlUrl = subjectApiUrl
            .Replace("https://api.github.com/repos/", "https://github.com/", StringComparison.OrdinalIgnoreCase)
            .Replace("/api/v3/repos/", "/", StringComparison.OrdinalIgnoreCase);

        // The REST API uses plural resource segments that differ from the web UI.
        htmlUrl = htmlUrl
            .Replace("/pulls/", "/pull/", StringComparison.Ordinal)
            .Replace("/commits/", "/commit/", StringComparison.Ordinal);

        return htmlUrl;
    }
}
