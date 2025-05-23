// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Octokit;

namespace GitHubExtension.DeveloperIds;

public class DeveloperId : IDeveloperId
{
    public string LoginId { get; private set; }

    public string DisplayName { get; private set; }

    public string Email { get; private set; }

    public string Url { get; private set; }

    public DateTime CredentialExpiryTime { get; set; }

    public IGitHubClient GitHubClient { get; private set; }

    public DeveloperId()
    {
        LoginId = string.Empty;
        DisplayName = string.Empty;
        Email = string.Empty;
        Url = string.Empty;
        GitHubClient = new GitHubClient(new ProductHeaderValue(Constants.CMDPAL_APPLICATION_NAME));
    }

    public DeveloperId(string loginId, string displayName, string email, string url, GitHubClient gitHubClient)
    {
        LoginId = loginId;
        DisplayName = displayName;
        Email = email;
        Url = url;
        GitHubClient = gitHubClient;
    }

    ~DeveloperId()
    {
        LoginId = string.Empty;
        DisplayName = string.Empty;
        Email = string.Empty;
        Url = string.Empty;
        return;
    }

    public Uri GetHostAddress() => GitHubClient.Connection.BaseAddress;
}
