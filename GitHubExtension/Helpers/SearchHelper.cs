// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.DataModel.Enums;

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

            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var searchQuery = queryParams["q"];

            if (!string.IsNullOrEmpty(searchQuery))
            {
                return searchQuery;
            }

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
