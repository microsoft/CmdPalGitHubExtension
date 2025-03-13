// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using Octokit;
using Serilog;

namespace GitHubExtension.PersistentData;

public class GitHubValidatorAdapter : IGitHubValidator
{
    private readonly IDeveloperIdProvider _developerIdProvider;

    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", nameof(OAuthRequest)));

    private static readonly ILogger _log = _logger.Value;

    public GitHubValidatorAdapter(IDeveloperIdProvider developerIdProvider)
    {
        _developerIdProvider = developerIdProvider;
    }

    public async Task ValidateSearch(ISearch search)
    {
        if (!_developerIdProvider.GetLoggedInDeveloperIdsInternal().Any())
        {
            // No users are signed in, cannot validate the search
            throw new NoSignedInUsersException("No GitHub client available for validation. User needs to sign in.");
        }

        var client = _developerIdProvider.GetLoggedInDeveloperIdsInternal().First().GitHubClient;

        // Type is set here so we can search issues and PRs by default
        var issuesOptions = new SearchIssuesRequest(search.SearchString)
        {
            Type = IssueTypeQualifier.Issue,
        };

        try
        {
            _ = await client.Search.SearchIssues(issuesOptions);
        }
        catch (Octokit.ApiException)
        {
            var repoInfo = GitHubHelper.ParseOwnerAndRepoFromSearchString(search.SearchString);

            var isSAMLException = await IsSAMLException(search, client, repoInfo);
            if (isSAMLException)
            {
                await LaunchSAMLLogin(repoInfo["owner"]);
                throw new InvalidOperationException("SAML authentication required. Please authenticate in the browser and try again.");
            }

            throw;
        }
    }

    private async Task<bool> LaunchSAMLLogin(string org)
    {
        var loginUrl = $"https://github.com/orgs/{org}/sso";

        var uri = new Uri(loginUrl);

        var browserLaunch = await Windows.System.Launcher.LaunchUriAsync(uri);

        if (browserLaunch)
        {
            _log.Information($"Uri Launched - Check browser");
            return true;
        }
        else
        {
            _log.Error($"Uri Launch failed");
            return false;
        }
    }

    private async Task<bool> IsSAMLException(ISearch search, GitHubClient client, Dictionary<string, string> repoInfo)
    {
        if (repoInfo.Count == 0)
        {
            // no repo info provided
            return false;
        }

        try
        {
            var repo = await client.Repository.Get(repoInfo["owner"], repoInfo["repo"]);

            // repo can be accessed without error
            return false;
        }
        catch (Octokit.ForbiddenException ex)
        {
            return ex.Message.Contains("SAML");
        }
    }
}
