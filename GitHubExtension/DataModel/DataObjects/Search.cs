// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.DataModel.DataObjects;

public class Search
{
    public string Name { get; set; } = string.Empty;

    public string SearchString { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public int Id { get; set; } = -1;

    public Search()
    {
    }

    public Search(string queryString)
    {
        SearchString = string.IsNullOrEmpty(queryString) ? string.Empty : queryString;
        Name = queryString;
    }

    public Search(string name, string type, string owner, string repository, string dateCreated, string language, string state, string reason, string numberOfComments, string labels, string author, string mentionedUsers, string assignee, string updatedDate)
    {
        // create a query string based on the fields passed in
        Name = name;
        Type = type;

        // check if fields are null or empty before adding to string
        var typeString = string.IsNullOrEmpty(type) ? string.Empty : $"is:{type.ToLower(System.Globalization.CultureInfo.CurrentCulture)} ";
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
