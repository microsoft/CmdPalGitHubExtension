// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Octokit;
using Serilog;

namespace GitHubExtension.DataModel.DataObjects;

public class PullRequest
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", $"DataModel/{nameof(PullRequest)}"));
    private static readonly ILogger _log = _logger.Value;

    public long Id { get; set; } = -1;

    public long Number { get; set; } = -1;

    public long AuthorId { get; set; } = -1;

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string HtmlUrl { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{Title} ({Number})";
    }

    // Create pull request from OctoKit issue data
    // If repository id is known at the time it can be supplied.
    public static PullRequest CreateFromOctokitIssue(Octokit.Issue octokitIssue)
    {
        return new PullRequest
        {
            Id = octokitIssue.Id,
            Number = octokitIssue.Number,
            AuthorId = octokitIssue.User.Id,
            Title = octokitIssue.Title ?? string.Empty,
            Body = octokitIssue.Body ?? string.Empty,
            State = octokitIssue.State.StringValue.ToString(),
            HtmlUrl = octokitIssue.HtmlUrl ?? string.Empty,
        };
    }
}
