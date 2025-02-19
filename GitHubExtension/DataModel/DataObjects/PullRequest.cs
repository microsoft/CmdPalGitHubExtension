// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Dapper;
using Dapper.Contrib.Extensions;
using GitHubExtension.Helpers;
using Serilog;

namespace GitHubExtension.DataModel;

[Table("PullRequest")]
public class PullRequest
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", $"DataModel/{nameof(PullRequest)}"));

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

    public string HeadSha { get; set; } = string.Empty;

    public long Merged { get; set; } = DataStore.NoForeignKey;

    public long Mergeable { get; set; } = DataStore.NoForeignKey;

    public string MergeableState { get; set; } = string.Empty;

    public long CommitCount { get; set; } = DataStore.NoForeignKey;

    public string HtmlUrl { get; set; } = string.Empty;

    public string SourceBranch { get; set; } = string.Empty;

    public long Locked { get; set; } = DataStore.NoForeignKey;

    public long Draft { get; set; } = DataStore.NoForeignKey;

    public long TimeCreated { get; set; } = DataStore.NoForeignKey;

    public long TimeUpdated { get; set; } = DataStore.NoForeignKey;

    public long TimeMerged { get; set; } = DataStore.NoForeignKey;

    public long TimeClosed { get; set; } = DataStore.NoForeignKey;

    public long TimeLastObserved { get; set; } = DataStore.NoForeignKey;

    // Label IDs are a string concatenation of Label internalIds.
    // We need to duplicate this data in order to properly do inserts and
    // to compare two objects for changes in order to add/remove associations.
    public string LabelIds { get; set; } = string.Empty;

    // Same use as Label IDs.
    public string AssigneeIds { get; set; } = string.Empty;

    [Write(false)]
    private DataStore? DataStore { get; set; }

    [Write(false)]
    [Computed]
    public DateTime CreatedAt => TimeCreated.ToDateTime();

    [Write(false)]
    [Computed]
    public DateTime UpdatedAt => TimeUpdated.ToDateTime();

    [Write(false)]
    [Computed]
    public DateTime MergedAt => TimeMerged.ToDateTime();

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
    public IEnumerable<Label> Labels
    {
        get
        {
            if (DataStore == null)
            {
                return Enumerable.Empty<Label>();
            }
            else
            {
                return PullRequestLabel.GetLabelsForPullRequest(DataStore, this) ?? Enumerable.Empty<Label>();
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
                return PullRequestAssign.GetUsersForPullRequest(DataStore, this) ?? Enumerable.Empty<User>();
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

    // Create pull request from OctoKit pull request data
    public static PullRequest CreateFromOctokitPullRequest(DataStore dataStore, Octokit.PullRequest okitPull, long repositoryId)
    {
        var pull = new PullRequest
        {
            DataStore = dataStore,
            InternalId = okitPull.Id,
            Number = okitPull.Number,
            Title = okitPull.Title ?? string.Empty,
            Body = okitPull.Body ?? string.Empty,
            SourceBranch = okitPull.Head.Label ?? string.Empty,
            State = okitPull.State.Value.ToString(),
            HeadSha = okitPull.Head.Sha ?? string.Empty,
            Merged = okitPull.Merged ? 1 : 0,
            Mergeable = (okitPull.Mergeable is not null && okitPull.Mergeable == true) ? 1 : 0,
            MergeableState = okitPull.MergeableState.HasValue ? okitPull.MergeableState.Value.ToString() : string.Empty,
            CommitCount = okitPull.Commits,
            HtmlUrl = okitPull.HtmlUrl ?? string.Empty,
            Locked = okitPull.Locked ? 1 : 0,
            Draft = okitPull.Draft ? 1 : 0,
            TimeCreated = okitPull.CreatedAt.DateTime.ToDataStoreInteger(),
            TimeUpdated = okitPull.UpdatedAt.DateTime.ToDataStoreInteger(),
            TimeMerged = okitPull.MergedAt.HasValue ? okitPull.MergedAt.Value.DateTime.ToDataStoreInteger() : 0,
            TimeClosed = okitPull.ClosedAt.HasValue ? okitPull.ClosedAt.Value.DateTime.ToDataStoreInteger() : 0,
            TimeLastObserved = DateTime.UtcNow.ToDataStoreInteger(),
        };

        // Labels are a string concat of label internal ids.
        var labels = new List<string>();
        foreach (var label in okitPull.Labels)
        {
            labels.Add(label.Id.ToStringInvariant());
            Label.GetOrCreateByOctokitLabel(dataStore, label);
        }

        pull.LabelIds = string.Join(",", labels);

        // Assignees are a string concat of User internal ids.
        var assignees = new List<string>();
        foreach (var user in okitPull.Assignees)
        {
            assignees.Add(user.Id.ToStringInvariant());
            User.GetOrCreateByOctokitUser(dataStore, user);
        }

        pull.AssigneeIds = string.Join(",", assignees);

        // Owner is a rowId in the User table
        var author = User.GetOrCreateByOctokitUser(dataStore, okitPull.User);
        pull.AuthorId = author.Id;

        // Repo is a row id in the Repository table.
        // It is likely the case that we already know the repository id (such as when querying pulls for a repository).
        if (repositoryId != DataStore.NoForeignKey)
        {
            pull.RepositoryId = repositoryId;
        }
        else if (okitPull.Base.Repository is not null)
        {
            // Use the base repository for the pull request.
            // This PR may be a private fork and Head and Base may be different.
            var repo = Repository.GetOrCreateByOctokitRepository(dataStore, okitPull.Base.Repository);
            pull.RepositoryId = repo.Id;
        }

        return pull;
    }

    private static PullRequest AddOrUpdatePullRequest(DataStore dataStore, PullRequest pull)
    {
        // Check for existing pull request data.
        var existingPull = GetByInternalId(dataStore, pull.InternalId);
        if (existingPull is not null)
        {
            // Existing pull requests must always be updated to update the LastObserved time.
            pull.Id = existingPull.Id;
            dataStore.Connection!.Update(pull);
            pull.DataStore = dataStore;

            if (pull.LabelIds != existingPull.LabelIds)
            {
                UpdateLabelsForPullRequest(dataStore, pull);
            }

            if (pull.AssigneeIds != existingPull.AssigneeIds)
            {
                UpdateAssigneesForPullRequest(dataStore, pull);
            }

            return pull;
        }

        // No existing pull request, add it.
        pull.Id = dataStore.Connection!.Insert(pull);

        // Now that we have an inserted Id, we can associate labels and assignees.
        UpdateLabelsForPullRequest(dataStore, pull);
        UpdateAssigneesForPullRequest(dataStore, pull);

        pull.DataStore = dataStore;

        return pull;
    }

    public static PullRequest? GetById(DataStore dataStore, long id)
    {
        var pull = dataStore.Connection!.Get<PullRequest>(id);
        if (pull is not null)
        {
            // Add Datastore so this object can make internal queries.
            pull.DataStore = dataStore;
        }

        return pull;
    }

    public static PullRequest? GetByInternalId(DataStore dataStore, long internalId)
    {
        var sql = @"SELECT * FROM PullRequest WHERE InternalId = @InternalId;";
        var param = new
        {
            InternalId = internalId,
        };

        var pull = dataStore.Connection!.QueryFirstOrDefault<PullRequest>(sql, param, null);
        if (pull is not null)
        {
            // Add Datastore so this object can make internal queries.
            pull.DataStore = dataStore;
        }

        return pull;
    }

    public static PullRequest GetOrCreateByOctokitPullRequest(DataStore dataStore, Octokit.PullRequest octokitPullRequest, long repositoryId = DataStore.NoForeignKey)
    {
        var newPull = CreateFromOctokitPullRequest(dataStore, octokitPullRequest, repositoryId);
        return AddOrUpdatePullRequest(dataStore, newPull);
    }

    public static IEnumerable<PullRequest> GetAllForRepository(DataStore dataStore, Repository repository)
    {
        var sql = @"SELECT * FROM PullRequest WHERE RepositoryId = @RepositoryId ORDER BY TimeUpdated DESC;";
        var param = new
        {
            RepositoryId = repository.Id,
        };

        _log.Verbose(DataStore.GetSqlLogMessage(sql, param));
        var pulls = dataStore.Connection!.Query<PullRequest>(sql, param, null) ?? Enumerable.Empty<PullRequest>();
        foreach (var pull in pulls)
        {
            pull.DataStore = dataStore;
        }

        return pulls;
    }

    public static IEnumerable<PullRequest> GetAllForUser(DataStore dataStore, User user)
    {
        var sql = @"SELECT * FROM PullRequest WHERE AuthorId = @AuthorId;";
        var param = new
        {
            AuthorId = user.Id,
        };

        _log.Verbose(DataStore.GetSqlLogMessage(sql, param));
        var pulls = dataStore.Connection!.Query<PullRequest>(sql, param, null) ?? Enumerable.Empty<PullRequest>();
        foreach (var pull in pulls)
        {
            pull.DataStore = dataStore;
        }

        return pulls;
    }

    private static void UpdateLabelsForPullRequest(DataStore dataStore, PullRequest pullRequest)
    {
        // Delete existing labels for this Pull Request and add new ones.
        PullRequestLabel.DeletePullRequestLabelsForPullRequest(dataStore, pullRequest);
        foreach (var label in pullRequest.LabelIds.Split(','))
        {
            if (long.TryParse(label, out var internalId))
            {
                var labelObj = Label.GetByInternalId(dataStore, internalId);
                if (labelObj is not null)
                {
                    PullRequestLabel.AddLabelToPullRequest(dataStore, pullRequest, labelObj);
                }
            }
        }
    }

    private static void UpdateAssigneesForPullRequest(DataStore dataStore, PullRequest pullRequest)
    {
        // Delete existing assignees for this Pull Request and add new ones.
        PullRequestAssign.DeletePullRequestAssignForPullRequest(dataStore, pullRequest);
        foreach (var user in pullRequest.AssigneeIds.Split(','))
        {
            if (long.TryParse(user, out var internalId))
            {
                var userObj = User.GetByInternalId(dataStore, internalId);
                if (userObj is not null)
                {
                    PullRequestAssign.AddUserToPullRequest(dataStore, pullRequest, userObj);
                }
            }
        }
    }

    // Delete records in a repository not observed before the specified date.
    public static void DeleteLastObservedBefore(DataStore dataStore, long repositoryId, DateTime date)
    {
        // Delete pull requests older than the time specified for the given repository.
        // This is intended to be run after updating a repository's Pull Requests so that non-observed
        // records will be removed.
        var sql = @"DELETE FROM PullRequest WHERE RepositoryId = $RepositoryId AND TimeLastObserved < $Time;";
        var command = dataStore.Connection!.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$Time", date.ToDataStoreInteger());
        command.Parameters.AddWithValue("$RepositoryId", repositoryId);
        _log.Verbose(DataStore.GetCommandLogMessage(sql, command));
        var rowsDeleted = command.ExecuteNonQuery();
        _log.Verbose(DataStore.GetDeletedLogMessage(rowsDeleted));
    }

    // Delete all records from a particular user before the specified date.
    // This is for removing developer pull requests across any repository that were not updated
    // recently. This should remove non-open pull requests from the developer across all repositories.
    public static void DeleteAllByDeveloperLoginAndLastObservedBefore(DataStore dataStore, string loginId, DateTime date)
    {
        var developerUsers = User.GetDeveloperUsers(dataStore);
        foreach (var user in developerUsers)
        {
            if (user.Login != loginId)
            {
                continue;
            }

            var sql = @"DELETE FROM PullRequest WHERE AuthorId = $UserId AND TimeLastObserved < $Time;";
            var command = dataStore.Connection!.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("$Time", date.ToDataStoreInteger());
            command.Parameters.AddWithValue("$UserId", user.Id);
            _log.Verbose(DataStore.GetCommandLogMessage(sql, command));
            var rowsDeleted = command.ExecuteNonQuery();
            _log.Verbose(DataStore.GetDeletedLogMessage(rowsDeleted));
            break;
        }
    }
}
