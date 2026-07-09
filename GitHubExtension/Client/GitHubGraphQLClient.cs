// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Serilog;

namespace GitHubExtension.Client;

// Sends GraphQL queries and mutations to GitHub using the logged-in developer's
// OAuth token. The GraphQL endpoint is derived from the REST client's base
// address so the same code path works for github.com and (later) GitHub
// Enterprise Server.
public sealed class GitHubGraphQLClient : IGitHubGraphQLClient
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", nameof(GitHubGraphQLClient)));

    private static readonly ILogger _log = _logger.Value;

    private static readonly HttpClient _httpClient = new();

    private readonly GitHubClientProvider _gitHubClientProvider;

    public GitHubGraphQLClient(GitHubClientProvider gitHubClientProvider)
    {
        _gitHubClientProvider = gitHubClientProvider;
    }

    public async Task<JsonNode?> QueryAsync(string query, object? variables = null)
    {
        var client = await _gitHubClientProvider.GetClientForLoggedInDeveloper(false);
        var token = client.Connection.Credentials?.Password
            ?? throw new InvalidOperationException("No authenticated GitHub token is available for GraphQL.");

        var endpoint = ResolveGraphQLEndpoint(client.Connection.BaseAddress);

        var payload = new Dictionary<string, object?>
        {
            ["query"] = query,
        };

        if (variables != null)
        {
            payload["variables"] = variables;
        }

        var body = JsonSerializer.Serialize(payload);

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue(Constants.CMDPAL_APPLICATION_NAME, "1.0"));

        using var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _log.Error($"GraphQL request failed with status {(int)response.StatusCode}: {content}");
            throw new HttpRequestException($"GraphQL request failed with status {(int)response.StatusCode}.");
        }

        var root = JsonNode.Parse(content);
        var errors = root?["errors"];
        if (errors is JsonArray errorArray && errorArray.Count > 0)
        {
            var message = errorArray[0]?["message"]?.ToString() ?? "Unknown GraphQL error.";
            _log.Error($"GraphQL response returned errors: {content}");
            throw new InvalidOperationException(message);
        }

        return root?["data"];
    }

    internal static Uri ResolveGraphQLEndpoint(Uri baseAddress)
    {
        return GitHubHostAddress.GetGraphQLUriFromApiBase(baseAddress);
    }
}
