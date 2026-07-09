// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Nodes;

namespace GitHubExtension.Client;

// Minimal GraphQL access for capabilities not covered by the Octokit REST
// client (Discussions, Projects v2, and auto-merge mutations). Returns the
// "data" node of the GraphQL response.
public interface IGitHubGraphQLClient
{
    Task<JsonNode?> QueryAsync(string query, object? variables = null);
}
