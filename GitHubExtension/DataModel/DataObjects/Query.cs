// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Nodes;

namespace GitHubExtension.DataModel.DataObjects;

public class Query
{
    public string Name { get; set; } = string.Empty;

    private string Owner { get; set; } = string.Empty;

    private string Repository { get; set; } = string.Empty;

    private string DateCreated { get; set; } = string.Empty;

    private string Language { get; set; } = string.Empty;

    private string State { get; set; } = string.Empty;

    private string Reason { get; set; } = string.Empty;

    private string NumberOfComments { get; set; } = string.Empty;

    private string Labels { get; set; } = string.Empty;

    private string Author { get; set; } = string.Empty;

    private string MentionedUsers { get; set; } = string.Empty;

    private string Assignees { get; set; } = string.Empty;

    private string UpdatedDate { get; set; } = string.Empty;

    public long AuthorId { get; set; } = -1;

    // id we'd use to identify the saved query
    private int Id { get; set; } = -1;

    public Query()
    {
    }

    public Query(string name, string owner, string repository, string dateCreated, string language, string state, string reason, string numberOfComments, string labels, string author, string mentionedUsers, string assignees, string updatedDate)
    {
        Name = name;
        Owner = owner;
        Repository = repository;
        DateCreated = dateCreated;
        Language = language;
        State = state;
        Reason = reason;
        NumberOfComments = numberOfComments;
        Labels = labels;
        Author = author;
        MentionedUsers = mentionedUsers;
        Assignees = assignees;
        UpdatedDate = updatedDate;
    }

    public override string ToString()
    {
        return $"Name: {Name}, Owner: {Owner}, Repository: {Repository}, DateCreated: {DateCreated}, Language: {Language}, State: {State}, Reason: {Reason}, NumberOfComments: {NumberOfComments}, Labels: {Labels}, Author: {Author}, MentionedUsers: {MentionedUsers}, Assignees: {Assignees}, UpdatedDate: {UpdatedDate}";
    }
}
