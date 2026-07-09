// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using System.Text.Json.Nodes;
using GitHubExtension.Client;
using GitHubExtension.Controls;
using Serilog;

namespace GitHubExtension.DataManager.Data;

public sealed class ProjectsDataManager : IProjectsDataManager
{
    private const int DefaultPageSize = 30;

    private const string MyProjectsQuery = @"
query($first: Int!) {
  viewer {
    projectsV2(first: $first, orderBy: {field: UPDATED_AT, direction: DESC}) {
      nodes {
        title
        url
        number
        closed
      }
    }
  }
}";

    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", nameof(ProjectsDataManager)));

    private static readonly ILogger _log = _logger.Value;

    private readonly IGitHubGraphQLClient _graphQLClient;

    public ProjectsDataManager(IGitHubGraphQLClient graphQLClient)
    {
        _graphQLClient = graphQLClient;
    }

    public async Task<IEnumerable<IProject>> GetMyProjectsAsync()
    {
        var variables = new Dictionary<string, object?>
        {
            ["first"] = DefaultPageSize,
        };

        var data = await _graphQLClient.QueryAsync(MyProjectsQuery, variables);
        var nodes = data?["viewer"]?["projectsV2"]?["nodes"] as JsonArray;

        if (nodes == null)
        {
            _log.Debug("Projects query returned no nodes.");
            return Array.Empty<IProject>();
        }

        var projects = new List<IProject>();
        foreach (var node in nodes)
        {
            if (node == null)
            {
                continue;
            }

            projects.Add(ToProject(node));
        }

        _log.Debug($"Projects query returned {projects.Count} projects.");
        return projects;
    }

    private static Project ToProject(JsonNode node)
    {
        return new Project
        {
            Title = node["title"]?.ToString() ?? string.Empty,
            HtmlUrl = node["url"]?.ToString() ?? string.Empty,
            Number = ParseLong(node["number"]),
            Closed = node["closed"]?.GetValue<bool>() ?? false,
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
}
