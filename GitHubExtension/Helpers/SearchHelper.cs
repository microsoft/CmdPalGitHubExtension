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
        type = searchString.Split(' ').FirstOrDefault(x => x.StartsWith("is:", StringComparison.OrdinalIgnoreCase));
        if (type != null)
        {
            var typeName = type.Split(':')[1];
            if (SearchTypeMappings.TryGetValue(typeName.ToLower(CultureInfo.CurrentCulture), out var searchType))
            {
                return searchType;
            }

            return (SearchType)Enum.Parse(typeof(SearchType), typeName, true);
        }

        return SearchType.IssuesAndPullRequests;
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
