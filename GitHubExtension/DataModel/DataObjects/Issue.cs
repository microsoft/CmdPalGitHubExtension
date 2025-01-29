// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Serilog;

namespace GitHubExtension.DataModel.DataObjects;

public class Issue
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", $"DataModel/{nameof(Issue)}"));

    private static readonly ILogger _log = _logger.Value;

    public long Id { get; set; } = -1;

    public long InternalId { get; set; } = -1;

    public long Number { get; set; } = -1;

    public long RepositoryId { get; set; } = -1;

    // User table
    public long AuthorId { get; set; } = -1;

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string HtmlUrl { get; set; } = string.Empty;

    // Label IDs are a string concatenation of Label internalIds.
    // We need to duplicate this data in order to properly do inserts and
    // to compare two objects for changes in order to add/remove associations.
    public string LabelIds { get; set; } = string.Empty;

    // Same use as Label IDs.
    public string AssigneeIds { get; set; } = string.Empty;

    public override string ToString() => $"{Number}: {Title}";

    // Create issue from OctoKit issue data
    // If repository id is known at the time it can be supplied.
    public static Issue CreateFromOctokitIssue(Octokit.Issue okitIssue)
    {
        var issue = new Issue
        {
            InternalId = okitIssue.Id,                      // Cannot be null.
            Number = okitIssue.Number,                      // Cannot be null.
            Title = okitIssue.Title ?? string.Empty,
            Body = okitIssue.Body ?? string.Empty,
            State = okitIssue.State.Value.ToString(),
            HtmlUrl = okitIssue.HtmlUrl ?? string.Empty,
        };

        // Owner is a row id in the User table
        var author = User.CreateFromOctokitUser(okitIssue.User);
        issue.AuthorId = author.Id;

        return issue;
    }
}
