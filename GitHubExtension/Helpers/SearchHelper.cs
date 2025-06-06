// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using System.Text.RegularExpressions;
using GitHubExtension.DataModel.Enums;
using Octokit;

namespace GitHubExtension.Helpers;

public static class SearchHelper
{
    public static SearchType ParseSearchTypeFromSearchString(string searchString)
    {
        // parse "type:typeName" if it's in the string
        var type = searchString.Split(' ').FirstOrDefault(x => x.StartsWith("type:", StringComparison.OrdinalIgnoreCase));
        if (type != null)
        {
            var typeName = type.Split(':')[1];
            if (SearchTypeMappings.TryGetValue(typeName.ToLower(CultureInfo.CurrentCulture), out var searchType))
            {
                return searchType;
            }

            return (SearchType)Enum.Parse(typeof(SearchType), typeName, true);
        }

        // parse "is:typeName" if it's in the string
        var isQualifiers = GetIsQualifiers(searchString);
        if (isQualifiers != null)
        {
            foreach (var isQualifier in isQualifiers)
            {
                try
                {
                    if (SearchTypeMappings.TryGetValue(isQualifier.ToLower(CultureInfo.CurrentCulture), out var searchType))
                    {
                        return searchType;
                    }
                }
                catch (ArgumentException)
                {
                    // Ignore the exception and continue
                }
            }
        }

        return SearchType.IssuesAndPullRequests;
    }

    public static IEnumerable<string> GetIsQualifiers(string searchString)
    {
        return searchString.Split(' ')
            .Where(x => x.StartsWith("is:", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Split(':')[1]);
    }

    public static string? ParseSearchStringFromUri(Uri uri)
    {
        try
        {
            var pathSegments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // case 1: a URL with a query string (e.g. "github.com?q=...")
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var searchQuery = queryParams["q"];
            var sort = queryParams["sort"];
            var order = queryParams["order"];

            if (!string.IsNullOrEmpty(searchQuery))
            {
                var searchBuilder = new List<string> { searchQuery };

                if (pathSegments.Length >= 2)
                {
                    var repoOwner = pathSegments[0];
                    var repoName = pathSegments[1];
                    searchBuilder.Insert(0, $"repo:{repoOwner}/{repoName}");
                }

                // Add sort if present
                if (!string.IsNullOrEmpty(sort))
                {
                    // Default order to desc if not specified
                    var sortOrder = string.IsNullOrEmpty(order) ? "desc" : order.ToLowerInvariant();
                    searchBuilder.Add($"sort:{sort}-{sortOrder}");
                }

                return string.Join(" ", searchBuilder);
            }

            // case 2: a URL that queries without a query string (e.g. "github.com/microsoft/PowerToys/issues")
            if (pathSegments.Length >= 2)
            {
                if (pathSegments.Length >= 3 &&
                    (string.Equals(pathSegments[2], "issues", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(pathSegments[2], "pulls", StringComparison.OrdinalIgnoreCase)))
                {
                    var searchBuilder = new List<string>();

                    searchBuilder.Add($"repo:{pathSegments[0]}/{pathSegments[1]}");

                    if (string.Equals(pathSegments[2], "issues", StringComparison.OrdinalIgnoreCase))
                    {
                        searchBuilder.Add("is:issue");
                    }
                    else if (string.Equals(pathSegments[2], "pulls", StringComparison.OrdinalIgnoreCase))
                    {
                        searchBuilder.Add("is:pr");
                    }

                    if (uri.Query.Contains("state=closed", StringComparison.OrdinalIgnoreCase))
                    {
                        searchBuilder.Add("is:closed");
                    }
                    else
                    {
                        searchBuilder.Add("is:open");
                    }

                    // Add sort if present
                    var sortParam = queryParams["sort"];
                    var orderParam = queryParams["order"];
                    if (!string.IsNullOrEmpty(sortParam))
                    {
                        var sortOrder = string.IsNullOrEmpty(orderParam) ? "desc" : orderParam.ToLowerInvariant();
                        searchBuilder.Add($"sort:{sortParam}-{sortOrder}");
                    }

                    return string.Join(" ", searchBuilder);
                }

                if (string.Equals(pathSegments[0], "search", StringComparison.OrdinalIgnoreCase))
                {
                    var searchBuilder = new List<string>();

                    if (pathSegments.Length > 1)
                    {
                        switch (pathSegments[1].ToLowerInvariant())
                        {
                            case "issues":
                                searchBuilder.Add("is:issue");
                                break;
                            case "pulls":
                                searchBuilder.Add("is:pr");
                                break;
                            case "repositories":
                                searchBuilder.Add("is:repo");
                                break;
                        }
                    }

                    // Add sort if present
                    var sortParam = queryParams["sort"];
                    var orderParam = queryParams["order"];
                    if (!string.IsNullOrEmpty(sortParam))
                    {
                        var sortOrder = string.IsNullOrEmpty(orderParam) ? "desc" : orderParam.ToLowerInvariant();
                        searchBuilder.Add($"sort:{sortParam}-{sortOrder}");
                    }

                    return string.Join(" ", searchBuilder);
                }
            }

            return null;
        }
        catch (UriFormatException)
        {
            return null;
        }
    }

    public static (IssueSearchSort SortField, SortDirection Direction, string UpdatedTerm)? ParseSortFromTerm(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return null;
        }

        // Regex to match sort:field-direction (e.g., sort:created-asc)
        var match = Regex.Match(term, @"sort:(\w+)-(asc|desc)", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return null;
        }

        var field = match.Groups[1].Value.ToLowerInvariant();
        var direction = match.Groups[2].Value.ToLowerInvariant();

        IssueSearchSort sortField;

        // reactions and interactions are not supported in the Octokit API
        switch (field)
        {
            case "created":
                sortField = IssueSearchSort.Created;
                break;
            case "updated":
                sortField = IssueSearchSort.Updated;
                break;
            case "comments":
                sortField = IssueSearchSort.Comments;
                break;
            default:
                return null;
        }

        var order = direction == "asc" ? SortDirection.Ascending : SortDirection.Descending;

        // Remove the sort:field-direction part from the term, preserving single spaces between terms
        var updatedTerm = Regex.Replace(term, @"\s*sort:\w+-\w+\s*", " ", RegexOptions.IgnoreCase)
            .Trim();

        // Replace multiple spaces with a single space
        updatedTerm = Regex.Replace(updatedTerm, @"\s{2,}", " ");

        return (sortField, order, updatedTerm);
    }

    private static readonly Dictionary<string, SearchType> SearchTypeMappings = new()
    {
        { "issue", SearchType.Issues },
        { "issues", SearchType.Issues },
        { "pr", SearchType.PullRequests },
        { "pullrequest", SearchType.PullRequests },
        { "repository", SearchType.Repositories },
        { "repo", SearchType.Repositories },
    };
}
