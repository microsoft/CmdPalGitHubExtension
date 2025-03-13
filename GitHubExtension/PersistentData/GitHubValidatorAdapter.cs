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
        // TODO: Change this request depending on the search type.
        GitHubClient? client = _developerIdProvider.GetLoggedInDeveloperIdsInternal().First().GitHubClient;

        if (!_developerIdProvider.GetLoggedInDeveloperIdsInternal().Any() || client == null)
        {
            // No client available, cannot validate the search.
            throw new InvalidOperationException("No GitHub client available for validation. User needs to sign in.");
        }

        var issuesOptions = new SearchIssuesRequest(search.SearchString)
        {
            Type = IssueTypeQualifier.Issue,
        };

        try
        {
            _ = await client.Search.SearchIssues(issuesOptions);
        }
        catch (Exception ex) when (ex is Octokit.ApiValidationException)
        {
            // check if the repo owner is an organization
            var repoInfo = GitHubHelper.ParseOwnerAndRepoFromSearchString(search.SearchString);

            if (repoInfo.Length == 2)
            {
                try
                {
                    var repo = await client.Repository.Get(repoInfo[0], repoInfo[1]);
                }
                catch (Exception ex2) when (ex2 is Octokit.ForbiddenException)
                {
                    var succeeded = await LaunchSAMLLogin(repoInfo[0]);
                    if (succeeded)
                    {
                        await ValidateSearch(search);
                    }
                }
            }
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
}
