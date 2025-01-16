// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Helpers;
using Octokit;

namespace GitHubExtension.Data;

public class GitHubClientProvider
{
    private readonly GitHubClient _publicRepoClient;

    private static GitHubClientProvider? _instance;

    public static GitHubClientProvider Instance => _instance ??= new GitHubClientProvider();

    public GitHubClientProvider()
    {
        var pat = GetPatFromFile();
        _publicRepoClient = new GitHubClient(new ProductHeaderValue("GitHubExtension"));
        var tokenAuth = new Credentials(pat);
        _publicRepoClient.Credentials = tokenAuth;
    }

    public GitHubClient GetClient() => _publicRepoClient;

    private string GetPatFromFile()
    {
        var settingsPath = GitHubHelper.StateJsonPath();

        // Check if the settings file exists
        if (!File.Exists(settingsPath))
        {
            return "notatoken";
        }

        // Read the file and parse the PAT
        var state = File.ReadAllText(settingsPath);
        var jsonState = System.Text.Json.Nodes.JsonNode.Parse(state);
        return jsonState?["pat"]?.ToString() ?? "notatoken";
    }
}
