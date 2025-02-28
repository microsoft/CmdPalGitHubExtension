// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataManager.Enums;

namespace GitHubExtension.Helpers;

public class EnumHelper
{
    public static string SearchCategoryToString(SearchCategory searchCategory) => searchCategory switch
    {
        SearchCategory.Issues => "Issues",
        SearchCategory.PullRequests => "PullRequests",
        SearchCategory.IssuesAndPullRequests => "IssuesAndPullRequests",
        _ => "unknown",
    };

    public static SearchCategory StringToSearchCategory(string value)
    {
        try
        {
            return Enum.Parse<SearchCategory>(value);
        }
        catch (Exception)
        {
            // Invalid value.
            return SearchCategory.Unknown;
        }
    }
}
