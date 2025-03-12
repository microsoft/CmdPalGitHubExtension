// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.DataModel.Enums;

namespace GitHubExtension.Helpers;

public class SearchCandidate : ISearch
{
    public string Name { get; set; } = string.Empty;

    public string SearchString { get; set; } = string.Empty;

    public SearchType Type { get; set; }

    public bool IsTopLevel { get; set; }

    public SearchCandidate()
    {
    }

    public SearchCandidate(string searchString)
    {
        SearchString = string.IsNullOrEmpty(searchString) ? string.Empty : searchString;
        Name = searchString;

        Type = SearchHelper.ParseSearchTypeFromSearchString(searchString);
    }

    public SearchCandidate(string searchString, string name)
    {
        SearchString = string.IsNullOrEmpty(searchString) ? string.Empty : searchString;
        Name = name;
        Type = SearchHelper.ParseSearchTypeFromSearchString(searchString);
    }

    public SearchCandidate(string searchString, string name, bool isTopLevel)
    {
        SearchString = string.IsNullOrEmpty(searchString) ? string.Empty : searchString;
        Name = name;
        Type = SearchHelper.ParseSearchTypeFromSearchString(searchString);
        IsTopLevel = isTopLevel;
    }
}
