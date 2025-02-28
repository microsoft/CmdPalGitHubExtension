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

    public SearchCandidate(string name, string type, string owner, string repository, string dateCreated, string language, string state, string reason, string numberOfComments, string labels, string author, string mentionedUsers, string assignee, string updatedDate)
    {
        // create a search string based on the fields passed in
        Name = name;

        Type = SearchHelper.ParseSearchTypeFromSearchString(type);

        // check if fields are null or empty before adding to string
        var typeString = string.IsNullOrEmpty(type) ? string.Empty : $"type:{type} ";
        var repositoryString = string.IsNullOrEmpty(repository) ? string.Empty : $"repo:{repository} ";
        var languageString = string.IsNullOrEmpty(language) ? string.Empty : $"language:{language} ";
        var stateString = string.IsNullOrEmpty(state) || string.Equals(state, "open/closed", StringComparison.OrdinalIgnoreCase) ? string.Empty : $"state:{state} ";
        var reasonString = string.IsNullOrEmpty(reason) || string.Equals(reason, "any reason", StringComparison.OrdinalIgnoreCase) ? string.Empty : $"reason:{reason} ";
        var numberOfCommentsString = string.IsNullOrEmpty(numberOfComments) ? string.Empty : $"comments:{numberOfComments} ";
        var labelsString = string.IsNullOrEmpty(labels) ? string.Empty : $"label:{labels} ";
        var authorString = string.IsNullOrEmpty(author) ? string.Empty : $"author:{author} ";
        var mentionedUsersString = string.IsNullOrEmpty(mentionedUsers) ? string.Empty : $"mentioned:{mentionedUsers} ";
        var assigneeString = string.IsNullOrEmpty(assignee) ? string.Empty : $"assignee:{assignee} ";
        var updatedDateString = string.IsNullOrEmpty(updatedDate) ? string.Empty : $"updated:{updatedDate} ";
        SearchString = $"{typeString} {repositoryString}{languageString}{stateString}{reasonString}{numberOfCommentsString}{labelsString}{authorString}{mentionedUsersString}{assigneeString}{updatedDateString}";
    }
}
