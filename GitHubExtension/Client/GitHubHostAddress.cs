// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.Client;

// Central resolver for GitHub host addresses. Given a user-entered host (for
// github.com or a GitHub Enterprise Server instance), it produces the Octokit
// REST API base address and the GraphQL endpoint, keeping all host-specific URL
// logic in one place so REST, GraphQL, and OAuth stay consistent.
public static class GitHubHostAddress
{
    private const string GitHubComApiBase = "https://api.github.com/";
    private const string GitHubComGraphQL = "https://api.github.com/graphql";

    // Returns true when the host is github.com (in any of its common forms).
    public static bool IsGitHubDotComHost(string host)
    {
        return host.Equals("github.com", StringComparison.OrdinalIgnoreCase)
            || host.Equals("www.github.com", StringComparison.OrdinalIgnoreCase)
            || host.Equals("api.github.com", StringComparison.OrdinalIgnoreCase);
    }

    // Normalizes a host string into an absolute Uri. Empty/whitespace resolves to
    // github.com. A bare host such as "github.example.com" is assumed https.
    public static Uri ParseHostInput(string? hostInput)
    {
        if (string.IsNullOrWhiteSpace(hostInput))
        {
            return new Uri("https://github.com");
        }

        var trimmed = hostInput.Trim();
        if (!trimmed.Contains("://", StringComparison.Ordinal))
        {
            trimmed = "https://" + trimmed;
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            throw new UriFormatException($"'{hostInput}' is not a valid host.");
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new UriFormatException($"'{hostInput}' must use http or https.");
        }

        return uri;
    }

    // The Octokit REST API base address for a host. github.com uses
    // https://api.github.com/; GitHub Enterprise Server uses https://HOST/api/v3/.
    public static Uri GetApiBaseUri(Uri host)
    {
        if (IsGitHubDotComHost(host.Host))
        {
            return new Uri(GitHubComApiBase);
        }

        return new Uri($"{host.Scheme}://{host.Authority}/api/v3/");
    }

    // The GraphQL endpoint for a host. github.com uses
    // https://api.github.com/graphql; GHES uses https://HOST/api/graphql.
    public static Uri GetGraphQLUri(Uri host)
    {
        if (IsGitHubDotComHost(host.Host))
        {
            return new Uri(GitHubComGraphQL);
        }

        return new Uri($"{host.Scheme}://{host.Authority}/api/graphql");
    }

    // Resolves the GraphQL endpoint from an Octokit REST base address (as exposed
    // by IConnection.BaseAddress), covering both github.com and GHES.
    public static Uri GetGraphQLUriFromApiBase(Uri apiBaseAddress)
    {
        if (IsGitHubDotComHost(apiBaseAddress.Host))
        {
            return new Uri(GitHubComGraphQL);
        }

        return new Uri($"{apiBaseAddress.Scheme}://{apiBaseAddress.Authority}/api/graphql");
    }
}
