// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using GitHubExtension.Client;
using GitHubExtension.PersistentData;

namespace GitHubExtension.Helpers;

public static class StringHelper
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

    public static string ParseHttpErrorMessage(string? httpBody)
    {
        if (string.IsNullOrEmpty(httpBody))
        {
            return string.Empty;
        }

        try
        {
            var jsonDocument = JsonDocument.Parse(httpBody);
            var root = jsonDocument.RootElement;

            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Array)
            {
                foreach (var error in errors.EnumerateArray())
                {
                    if (error.TryGetProperty("message", out var message))
                    {
                        return message.GetString() ?? string.Empty;
                    }
                }
            }
        }
        catch (JsonException)
        {
            // Handle JSON parsing errors if necessary
        }

        return string.Empty;
    }
}
