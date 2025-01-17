// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Octokit;
using Serilog;

namespace GitHubExtension.Client;

// Validation layer to help parsing GitHub URL.
public static class Validation
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", nameof(Validation)));

    private static readonly ILogger _log = _logger.Value;

    private static bool IsValidHttpUri(string uriString, out Uri? uri)
    {
        return Uri.TryCreate(uriString, UriKind.Absolute, out uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    public static bool IsValidGitHubURL(Uri uri) => IsValidGitHubComURL(uri) || (IsValidGitHubEnterpriseServerURL(uri) && IsReachableGitHubEnterpriseServerURL(uri).Result);

    public static bool IsValidGitHubComURL(Uri uri)
    {
        // Valid GitHub URL has three segments.  The first is '/'.
        if (uri.Segments.Length < 3 || (!uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase) && !uri.Host.Equals("www.github.com", StringComparison.OrdinalIgnoreCase)))
        {
            _log.Debug($"{uri.OriginalString} is not a valid GitHub uri");
            return false;
        }

        return true;
    }

    public static bool IsValidGitHubEnterpriseServerURL(Uri server)
    {
        // Valid GHES URL has three segments.
        // There are no restrictions on the hostname, except what is covered in IsValidHttpUri()
        // https://docs.github.com/en/enterprise-server@3.10/admin/configuration/configuring-network-settings/configuring-the-hostname-for-your-instance
        if (server.Segments.Length < 3)
        {
            _log.Debug($"{server.OriginalString} is not a valid GHES repo uri");
            return false;
        }

        return true;
    }

    // Ensure it is a GitHub repo URL.
    public static bool IsValidGitHubURL(string url)
    {
        Uri? parsedUri;

        // https://github.com/dotnet/runtime/issues/72632
        // IsWellFormedUriString returns false with a GitHub URL.
        // Above link shows a work around.
        if (!IsValidHttpUri(url, out parsedUri) || url == null || parsedUri == null)
        {
            _log.Debug($"{url} is not a valid http uri");
            return false;
        }

        return IsValidGitHubURL(parsedUri);
    }

    public static bool IsValidGitHubIssueQueryURL(string url)
    {
        if (!IsValidGitHubURL(url))
        {
            return false;
        }

        Uri? uri;
        return Uri.TryCreate(url, UriKind.Absolute, out uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
&& uri.Segments.Length >= 4 && uri.Segments[3].Equals("issues", StringComparison.OrdinalIgnoreCase);
    }

    public static Uri GetUriFromGitHubUrlString(string url)
    {
        if (!IsValidGitHubURL(url))
        {
            // Try adding a protocol to support just "github.com/owner/repo" type inputs.
            var urlWithProtocol = AddProtocolToString(url);
            if (!IsValidGitHubURL(urlWithProtocol))
            {
                throw new InvalidGitHubUrlException($"{url} is invalid.");
            }

            url = urlWithProtocol;
        }

        return new Uri(RemoveDotGitFromEndOfString(url));
    }

    public static string ParseOwnerFromGitHubURL(string url)
    {
        // Check if URL string provided as just the repository FullName.
        var fullNameSplit = GetNameAndRepoFromFullName(url);
        return fullNameSplit is not null ? fullNameSplit[0] : ParseOwnerFromGitHubURL(GetUriFromGitHubUrlString(url));
    }

    // New method to parse owner from GitHub API URL
    public static string ParseOwnerFromGitHubApiUrl(string apiUrl)
    {
        if (!Uri.TryCreate(apiUrl, UriKind.Absolute, out var uri))
        {
            throw new InvalidGitHubUrlException($"{apiUrl} is not a valid URI.");
        }

        var segments = uri.Segments;
        return segments.Length < 4 || !segments[1].Equals("repos/", StringComparison.OrdinalIgnoreCase)
            ? throw new InvalidGitHubUrlException($"{apiUrl} is not a valid GitHub API URL.")
            : segments[2].TrimEnd('/');
    }

#pragma warning disable IDE0022 // Use expression body for method
    public static string ParseOwnerFromGitHubURL(Uri url)
    {
        // For some reason Segments is returning trailing '/', even though the documentation
        // remarks state that it strips out the separator. This is a fix for that which will
        // work even if/when that issue is fixed.
        return url.Segments[1].Replace("/", string.Empty);
    }
#pragma warning restore IDE0022 // Use expression body for method

    public static string ParseRepositoryFromGitHubURL(string url)
    {
        // Check if URL string provided as just the repository FullName.
        var fullNameSplit = GetNameAndRepoFromFullName(url);
        return fullNameSplit is not null ? fullNameSplit[1] : ParseRepositoryFromGitHubURL(GetUriFromGitHubUrlString(url));
    }

    public static string ParseRepositoryFromGitHubURL(Uri url)
    {
        // Replace .git because Octokit does not want .git.
        var repoName = url.Segments[2].Replace("/", string.Empty);
        return RemoveDotGitFromEndOfString(repoName);
    }

    public static string ParseIssueQueryFromGitHubURL(string url)
    {
        if (!IsValidGitHubIssueQueryURL(url))
        {
            return string.Empty;
        }

        var uri = new Uri(url);

        // Query includes the ?q= prefix, which we need to remove and return the raw query string.
        return uri.Query.StartsWith(@"?q=", StringComparison.OrdinalIgnoreCase) ? uri.Query[3..] : string.Empty;
    }

    /// <summary>
    /// Removes either .git or .git/ from the end of the string.  If the string doe not end with
    /// .git or .git/ the original string is returned unmodified.
    /// </summary>
    /// <param name="stringWithDotGit">The string to parse</param>
    /// <returns>stringWithDotGit with .git or .git/ removed from the end.</returns>
    private static string RemoveDotGitFromEndOfString(string stringWithDotGit)
    {
        if (stringWithDotGit.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
        {
            var locationOfLastDotGit = stringWithDotGit.LastIndexOf(".git", StringComparison.OrdinalIgnoreCase);
            return stringWithDotGit.Remove(locationOfLastDotGit);
        }
        else if (stringWithDotGit.EndsWith(".git/", StringComparison.OrdinalIgnoreCase))
        {
            var locationOfLastDotGit = stringWithDotGit.LastIndexOf(".git/", StringComparison.OrdinalIgnoreCase);
            return stringWithDotGit.Remove(locationOfLastDotGit);
        }

        return stringWithDotGit;
    }

    public static string ParseFullNameFromGitHubURL(string url)
    {
        // Check if URL string provided as just the repository FullName.
        var fullNameSplit = GetNameAndRepoFromFullName(url);
        return fullNameSplit is not null ? url : ParseFullNameFromGitHubURL(GetUriFromGitHubUrlString(url));
    }

    public static string ParseFullNameFromGitHubURL(Uri url) =>

        // Need to account for the presence or absence of a trailing '/' in the segments, and
        // ensure there is exactly one slash separator in the full name.
        $"{url.Segments[1].Replace("/", string.Empty)}/{url.Segments[2].Replace("/", string.Empty)}";

    // Adds a protocol to a string to allow for protocol-less Uris.
    private static string AddProtocolToString(string s) => "https://" + s;

    private static string[]? GetNameAndRepoFromFullName(string s)
    {
        var n = s.Split(['/']);

        // This should be exactly two results with no empty strings.
        return n.Length != 2 || string.IsNullOrEmpty(n[0]) || string.IsNullOrEmpty(n[1]) ? null : n;
    }

    public static async Task<bool> IsReachableGitHubEnterpriseServerURL(Uri server)
    {
        try
        {
            var probeResult = await new EnterpriseProbe(new ProductHeaderValue(Constants.CMDPAL_APPLICATION_NAME)).Probe(server);
            if (probeResult != EnterpriseProbeResult.Ok)
            {
                _log.Error($"EnterpriseServer {server.AbsoluteUri} is not reachable");
                return false;
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"EnterpriseServer {server.AbsoluteUri} could not be probed.");
            return false;
        }

        return true;
    }
}
