// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using System.Text.Json.Nodes;
using GitHubExtension.Client;
using GitHubExtension.Controls;
using Serilog;

namespace GitHubExtension.DataManager.Data;

public sealed class DiscussionsDataManager : IDiscussionsDataManager
{
    private const int DefaultPageSize = 30;

    private const string SearchQuery = @"
query($q: String!, $first: Int!) {
  search(query: $q, type: DISCUSSION, first: $first) {
    nodes {
      ... on Discussion {
        title
        url
        number
        updatedAt
        repository { nameWithOwner }
        author { login }
      }
    }
  }
}";

    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", nameof(DiscussionsDataManager)));

    private static readonly ILogger _log = _logger.Value;

    private readonly IGitHubGraphQLClient _graphQLClient;

    public DiscussionsDataManager(IGitHubGraphQLClient graphQLClient)
    {
        _graphQLClient = graphQLClient;
    }

    public async Task<IEnumerable<IDiscussion>> SearchDiscussionsAsync(string query)
    {
        var variables = new Dictionary<string, object?>
        {
            ["q"] = query,
            ["first"] = DefaultPageSize,
        };

        var data = await _graphQLClient.QueryAsync(SearchQuery, variables);
        var nodes = data?["search"]?["nodes"] as JsonArray;

        if (nodes == null)
        {
            _log.Debug("Discussion search returned no nodes.");
            return Array.Empty<IDiscussion>();
        }

        var discussions = new List<IDiscussion>();
        foreach (var node in nodes)
        {
            if (node == null)
            {
                continue;
            }

            discussions.Add(ToDiscussion(node));
        }

        _log.Debug($"Discussion search returned {discussions.Count} discussions.");
        return discussions;
    }

    private static Discussion ToDiscussion(JsonNode node)
    {
        return new Discussion
        {
            Title = node["title"]?.ToString() ?? string.Empty,
            HtmlUrl = node["url"]?.ToString() ?? string.Empty,
            Number = ParseLong(node["number"]),
            RepositoryFullName = node["repository"]?["nameWithOwner"]?.ToString() ?? string.Empty,
            Author = node["author"]?["login"]?.ToString() ?? string.Empty,
            UpdatedAt = ParseDate(node["updatedAt"]?.ToString()),
        };
    }

    private static long ParseLong(JsonNode? node)
    {
        if (node != null && long.TryParse(node.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        return 0;
    }

    private static DateTimeOffset ParseDate(string? value)
    {
        if (!string.IsNullOrEmpty(value) &&
            DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed))
        {
            return parsed;
        }

        return DateTimeOffset.MinValue;
    }
}
