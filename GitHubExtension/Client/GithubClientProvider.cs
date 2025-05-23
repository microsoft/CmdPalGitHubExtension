// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DeveloperIds;
using Octokit;
using Serilog;

namespace GitHubExtension.Client;

public class GitHubClientProvider
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", nameof(GitHubClientProvider)));

    private static readonly ILogger _log = _logger.Value;

    private readonly GitHubClient _publicRepoClient;

    private static readonly Lock _instanceLock = new();

    private readonly IDeveloperIdProvider _developerIdProvider;

    public GitHubClientProvider(IDeveloperIdProvider developerIdProvider)
    {
        _developerIdProvider = developerIdProvider;
        _publicRepoClient = new GitHubClient(new ProductHeaderValue(Constants.CMDPAL_APPLICATION_NAME));
    }

    public IGitHubClient? GetClient(IDeveloperId devId)
    {
        var devIdInternal = _developerIdProvider.GetDeveloperIdInternal(devId) ?? throw new ArgumentException(devId.LoginId);
        return devIdInternal.GitHubClient;
    }

    public IGitHubClient GetClient(string url)
    {
        var devIdInternal = _developerIdProvider.GetLoggedInDeveloperIdsInternal().Where(i => i.Url.Equals(url, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        return devIdInternal == null ? _publicRepoClient : devIdInternal.GitHubClient;
    }

    public GitHubClient GetClient() => _publicRepoClient;

    public bool IsClientLoggedIn(GitHubClient client)
    {
        return !GitHubClient.Equals(client, _publicRepoClient);
    }

    public async Task<IGitHubClient> GetClientForLoggedInDeveloper(bool logRateLimit = false)
    {
        var devIds = _developerIdProvider.GetLoggedInDeveloperIdsInternal();

        IGitHubClient client;
        if (devIds == null || !devIds.Any())
        {
            _log.Information($"No logged in developer, using public GitHub client.");
            client = GetClient();
        }
        else
        {
            _log.Information($"Using authenticated user: {devIds.First().LoginId}");
            client = devIds.First().GitHubClient;
        }

        if (client == null)
        {
            _log.Error($"Failed creating GitHubClient.");
            return client!;
        }

        if (logRateLimit)
        {
            try
            {
                var miscRateLimit = await client.RateLimit.GetRateLimits();
                _log.Information($"Rate Limit:  Remaining: {miscRateLimit.Resources.Core.Remaining}  Total: {miscRateLimit.Resources.Core.Limit}  Resets: {miscRateLimit.Resources.Core.Reset}");
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"Rate limiting not enabled for server.");
            }
        }

        return client;
    }
}
