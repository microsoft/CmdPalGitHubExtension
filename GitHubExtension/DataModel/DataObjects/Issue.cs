// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Dapper;
using Dapper.Contrib.Extensions;
using GitHubExtension.Controls.Pages;
using GitHubExtension.Helpers;
using Serilog;

namespace GitHubExtension.DataModel.DataObjects;

[Table("Issue")]
public class Issue : IIssue
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", $"DataModel/{nameof(Issue)}"));

    private static readonly ILogger _log = _logger.Value;

    [Key]
    public long Id { get; set; } = DataStore.NoForeignKey;

    public long InternalId { get; set; } = DataStore.NoForeignKey;

    public long Number { get; set; } = DataStore.NoForeignKey;

    // Repository table
    public long RepositoryId { get; set; } = DataStore.NoForeignKey;

    // User table
    public long AuthorId { get; set; } = DataStore.NoForeignKey;

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string HtmlUrl { get; set; } = string.Empty;

    public long Locked { get; set; } = DataStore.NoForeignKey;

    public long TimeCreated { get; set; } = DataStore.NoForeignKey;

    public long TimeUpdated { get; set; } = DataStore.NoForeignKey;

    public long TimeClosed { get; set; } = DataStore.NoForeignKey;

    public long TimeLastObserved { get; set; } = DataStore.NoForeignKey;

    // Label IDs are a string concatenation of Label internalIds.
    // We need to duplicate this data in order to properly do inserts and
    // to compare two objects for changes in order to add/remove associations.
    public string LabelIds { get; set; } = string.Empty;

    // Same use as Label IDs.
    public string AssigneeIds { get; set; } = string.Empty;

    [Write(false)]
    private DataStore? DataStore
    {
        get; set;
    }

    [Write(false)]
    [Computed]
    public DateTime CreatedAt => TimeCreated.ToDateTime();

    [Write(false)]
    [Computed]
    public DateTime UpdatedAt => TimeUpdated.ToDateTime();

    [Write(false)]
    [Computed]
    public DateTime ClosedAt => TimeClosed.ToDateTime();

    [Write(false)]
    [Computed]
    public DateTime LastObservedAt => TimeLastObserved.ToDateTime();

    // Derived Properties so consumers of these objects do not
    // need to do further queries of the datastore.
    [Write(false)]
    [Computed]
    public IEnumerable<ILabel> Labels
    {
        get
        {
            if (DataStore == null)
            {
                return Enumerable.Empty<Label>();
            }
            else
            {
                return IssueLabel.GetLabelsForIssue(DataStore, this) ?? Enumerable.Empty<Label>();
            }
        }
    }

    [Write(false)]
    [Computed]
    public IEnumerable<User> Assignees
    {
        get
        {
            if (DataStore == null)
            {
                return Enumerable.Empty<User>();
            }
            else
            {
                return IssueAssign.GetUsersForIssue(DataStore, this) ?? Enumerable.Empty<User>();
            }
        }
    }

    [Write(false)]
    [Computed]
    public Repository Repository
    {
        get
        {
            if (DataStore == null)
            {
                return new Repository();
            }
            else
            {
                return Repository.GetById(DataStore, RepositoryId) ?? new Repository();
            }
        }
    }

    [Write(false)]
    [Computed]
    public User Author
    {
        get
        {
            if (DataStore == null)
            {
                return new User();
            }
            else
            {
                return User.GetById(DataStore, AuthorId) ?? new User();
            }
        }
    }

    public override string ToString() => $"{Number}: {Title}";

    // Create issue from OctoKit issue data
    // If repository id is known at the time it can be supplied.
    public static Issue CreateFromOctokitIssue(DataStore dataStore, Octokit.Issue okitIssue, long repositoryId)
    {
        var issue = new Issue
        {
            DataStore = dataStore,
            InternalId = okitIssue.Id,                      // Cannot be null.
            Number = okitIssue.Number,                      // Cannot be null.
            Title = okitIssue.Title ?? string.Empty,
            Body = okitIssue.Body ?? string.Empty,
            State = okitIssue.State.Value.ToString(),
            HtmlUrl = okitIssue.HtmlUrl ?? string.Empty,
            Locked = okitIssue.Locked ? 1 : 0,
            TimeCreated = okitIssue.CreatedAt.DateTime.ToDataStoreInteger(),
            TimeUpdated = okitIssue.UpdatedAt.HasValue ? okitIssue.UpdatedAt.Value.DateTime.ToDataStoreInteger() : 0,
            TimeClosed = okitIssue.ClosedAt.HasValue ? okitIssue.ClosedAt.Value.DateTime.ToDataStoreInteger() : 0,
            TimeLastObserved = DateTime.UtcNow.ToDataStoreInteger(),
        };

        // Labels are a string concat of label internal ids.
        var labels = new List<string>();
        foreach (var label in okitIssue.Labels)
        {
            // Add label to the list of this issue, and add it to the datastore.
            // We cannot associate label to this issue until this issue is actually
            // inserted into the datastore.
            labels.Add(label.Id.ToStringInvariant());
            Label.GetOrCreateByOctokitLabel(dataStore, label);
        }

        issue.LabelIds = string.Join(",", labels);

        // Assignees are a string concat of User internal ids.
        var assignees = new List<string>();
        foreach (var user in okitIssue.Assignees)
        {
            assignees.Add(user.Id.ToStringInvariant());
            User.GetOrCreateByOctokitUser(dataStore, user);
        }

        issue.AssigneeIds = string.Join(",", assignees);

        // Owner is a row id in the User table
        var author = User.GetOrCreateByOctokitUser(dataStore, okitIssue.User);
        issue.AuthorId = author.Id;

        // Repo is a row id in the Repository table.
        // It is likely the case that we already know the repository id (such as when querying issues for a repository).
        // In addition, the Octokit Issue data object may not contain repository information. To work around this null
        // data, we have an optional repositoryId that can be supplied that saves us the lookup time.
        if (repositoryId != DataStore.NoForeignKey)
        {
            issue.RepositoryId = repositoryId;
        }
        else if (okitIssue.Repository is not null)
        {
            var repo = Repository.GetOrCreateByOctokitRepository(dataStore, okitIssue.Repository);
            issue.RepositoryId = repo.Id;
        }

        return issue;
    }

    private static Issue AddOrUpdateIssue(DataStore dataStore, Issue issue)
    {
        // Check for existing issue data.
        var existing = GetByInternalId(dataStore, issue.InternalId);
        if (existing is not null)
        {
            // Existing issues must be updated and always marked observed.
            issue.Id = existing.Id;
            dataStore.Connection!.Update(issue);
            issue.DataStore = dataStore;

            if (issue.LabelIds != existing.LabelIds)
            {
                UpdateLabelsForIssue(dataStore, issue);
            }

            if (issue.AssigneeIds != existing.AssigneeIds)
            {
                UpdateAssigneesForIssue(dataStore, issue);
            }

            return issue;
        }

        // No existing issue, add it.
        issue.Id = dataStore.Connection!.Insert(issue);

        // Now that we have an inserted Id, we can associate labels and assignees.
        UpdateLabelsForIssue(dataStore, issue);
        UpdateAssigneesForIssue(dataStore, issue);

        issue.DataStore = dataStore;
        return issue;
    }

    public static Issue? GetById(DataStore dataStore, long id)
    {
        var issue = dataStore.Connection!.Get<Issue>(id);
        if (issue is not null)
        {
            // Add Datastore so this object can make internal queries.
            issue.DataStore = dataStore;
        }

        return issue;
    }

    public static Issue? GetByInternalId(DataStore dataStore, long internalId)
    {
        var sql = $"SELECT * FROM Issue WHERE InternalId = @InternalId;";
        var param = new
        {
            InternalId = internalId,
        };

        var issue = dataStore.Connection!.QueryFirstOrDefault<Issue>(sql, param, null);
        if (issue is not null)
        {
            // Add Datastore so this object can make internal queries.
            issue.DataStore = dataStore;
        }

        return issue;
    }

    public static Issue GetOrCreateByOctokitIssue(DataStore dataStore, Octokit.Issue octokitIssue, long repositoryId = DataStore.NoForeignKey)
    {
        var issue = CreateFromOctokitIssue(dataStore, octokitIssue, repositoryId);
        return AddOrUpdateIssue(dataStore, issue);
    }

    public static IEnumerable<Issue> GetAllForRepository(DataStore dataStore, Repository repository)
    {
        var sql = $"SELECT * FROM Issue WHERE RepositoryId = @RepositoryId ORDER BY TimeCreated DESC;";
        var param = new
        {
            RepositoryId = repository.Id,
        };

        _log.Verbose(DataStore.GetSqlLogMessage(sql, param));
        var issues = dataStore.Connection!.Query<Issue>(sql, param, null) ?? Enumerable.Empty<Issue>();
        foreach (var issue in issues)
        {
            issue.DataStore = dataStore;
        }

        return issues;
    }

    public static IEnumerable<Issue> GetForSearch(DataStore dataStore, Search search)
    {
        // Order the resulting set by TimeUpdated on the SearchIssue table. Items returned first in
        // a search result will be processed first, and added first to the datastore. This means the
        // newest timestamp entry is the last one in the list. So we must order the results by time
        // updated, but ascending to get them in the order in which they were received in the search.
        // This is how we preserve whatever ordering the search had for these items without knowing
        // what that search ordering actually was.
        var sql = @"SELECT * FROM Issue WHERE Id IN (SELECT Issue FROM SearchIssue WHERE Search = @SearchId ORDER BY TimeUpdated ASC)";
        var param = new
        {
            SearchId = search.Id,
        };

        _log.Verbose(DataStore.GetSqlLogMessage(sql, param));
        var issues = dataStore.Connection!.Query<Issue>(sql, param, null) ?? Enumerable.Empty<Issue>();
        foreach (var issue in issues)
        {
            issue.DataStore = dataStore;
        }

        return issues;
    }

    private static void UpdateLabelsForIssue(DataStore dataStore, Issue issue)
    {
        // Delete existing labels for this issue and add new ones.
        IssueLabel.DeleteIssueLabelsForIssue(dataStore, issue);
        foreach (var label in issue.LabelIds.Split(','))
        {
            if (long.TryParse(label, out var internalId))
            {
                var labelObj = Label.GetByInternalId(dataStore, internalId);
                if (labelObj is not null)
                {
                    IssueLabel.AddLabelToIssue(dataStore, issue, labelObj);
                }
            }
        }
    }

    private static void UpdateAssigneesForIssue(DataStore dataStore, Issue issue)
    {
        // Delete existing assignees for this issue and add new ones.
        IssueAssign.DeleteIssueAssignForIssue(dataStore, issue);
        foreach (var user in issue.AssigneeIds.Split(','))
        {
            if (long.TryParse(user, out var internalId))
            {
                var userObj = User.GetByInternalId(dataStore, internalId);
                if (userObj is not null)
                {
                    IssueAssign.AddUserToIssue(dataStore, issue, userObj);
                }
            }
        }
    }

    // Delete records in a repository not observed before the specified date.
    public static void DeleteLastObservedBefore(DataStore dataStore, long repositoryId, DateTime date)
    {
        // Delete issues older than the time specified for the given repository.
        // This is intended to be run after updating a repository's issues so that non-observed
        // records will be removed.
        var sql = @"DELETE FROM Issue WHERE RepositoryId = $RepositoryId AND TimeLastObserved < $Time;";
        var command = dataStore.Connection!.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$Time", date.ToDataStoreInteger());
        command.Parameters.AddWithValue("$RepositoryId", repositoryId);
        _log.Verbose(DataStore.GetCommandLogMessage(sql, command));
        var rowsDeleted = command.ExecuteNonQuery();
        _log.Verbose(DataStore.GetDeletedLogMessage(rowsDeleted));
    }
}
