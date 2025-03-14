// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Dapper;
using Dapper.Contrib.Extensions;
using GitHubExtension.Controls;
using GitHubExtension.Helpers;
using Serilog;

namespace GitHubExtension.DataModel.DataObjects;

[Table("PullRequest")]
public class PullRequest : IPullRequest
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

    // For getting pull request for a search, the only API available is the issues API.
    // This means we have to create a pull request from an issue to be efficient on our
    // API calls. That way, if we need other types of data for a pull request, we can
    // use the Pull Requests API to update the data individually later.
    public static PullRequest CreateFromOctokitIssue(DataStore dataStore, Octokit.Issue okitIssue, long repositoryId)
    {
        var pull = new PullRequest
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

        pull.LabelIds = string.Join(",", labels);

        // Assignees are a string concat of User internal ids.
        var assignees = new List<string>();
        foreach (var user in okitIssue.Assignees)
        {
            assignees.Add(user.Id.ToStringInvariant());
            User.GetOrCreateByOctokitUser(dataStore, user);
        }

        pull.AssigneeIds = string.Join(",", assignees);

        // Owner is a row id in the User table
        var author = User.GetOrCreateByOctokitUser(dataStore, okitIssue.User);
        pull.AuthorId = author.Id;

        // Repo is a row id in the Repository table.
        // It is likely the case that we already know the repository id (such as when querying issues for a repository).
        // In addition, the Octokit Issue data object may not contain repository information. To work around this null
        // data, we have an optional repositoryId that can be supplied that saves us the lookup time.
        if (repositoryId != DataStore.NoForeignKey)
        {
            pull.RepositoryId = repositoryId;
        }
        else if (okitIssue.Repository is not null)
        {
            var repo = Repository.GetOrCreateByOctokitRepository(dataStore, okitIssue.Repository);
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

    public static PullRequest GetOrCreateByOctokitIssue(DataStore dataStore, Octokit.Issue octokitIssue, long repositoryId = DataStore.NoForeignKey)
    {
        var newPull = CreateFromOctokitIssue(dataStore, octokitIssue, repositoryId);
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

    public static IEnumerable<PullRequest> GetForSearch(DataStore dataStore, Search search)
    {
        // Order the resulting set by TimeUpdated on the SearchIssue table. Items returned first in
        // a search result will be processed first, and added first to the datastore. This means the
        // newest timestamp entry is the last one in the list. So we must order the results by time
        // updated, but ascending to get them in the order in which they were received in the search.
        // This is how we preserve whatever ordering the search had for these items without knowing
        // what that search ordering actually was.
        var sql = @"SELECT * FROM PullRequest WHERE Id IN (SELECT PullRequest FROM SearchPullRequest WHERE Search = @SearchId ORDER BY TimeUpdated ASC)";
        var param = new
        {
            SearchId = search.Id,
        };

        _log.Verbose(DataStore.GetSqlLogMessage(sql, param));
        var pullRequests = dataStore.Connection!.Query<PullRequest>(sql, param, null) ?? Enumerable.Empty<PullRequest>();
        foreach (var pullRequest in pullRequests)
        {
            pullRequest.DataStore = dataStore;
        }

        return pullRequests;
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

    public static void DeleteLastObservedBefore(DataStore dataStore, long searchId, DateTime date)
    {
        // Delete pull requests older than the time specified for the given search.
        // This is intended to be run after updating a search's Pull Requests so that non-observed
        // records will be removed.
        var sql = @"DELETE FROM PullRequest WHERE Id IN (SELECT PullRequest FROM SearchPullRequest WHERE Search = $SearchId) AND TimeLastObserved < $Time;";
        var command = dataStore.Connection!.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$Time", date.ToDataStoreInteger());
        command.Parameters.AddWithValue("$SearchId", searchId);
        _log.Verbose(DataStore.GetCommandLogMessage(sql, command));
        var rowsDeleted = command.ExecuteNonQuery();
        _log.Verbose(DataStore.GetDeletedLogMessage(rowsDeleted));
    }

    public static void DeleteNotReferencedBySearch(DataStore dataStore)
    {
        // Delete pull requests that are not referenced by any search.
        var sql = @"DELETE FROM PullRequest WHERE Id NOT IN (SELECT PullRequest FROM SearchPullRequest);";
        var rowsDeleted = dataStore.Connection!.Execute(sql);
        _log.Verbose(DataStore.GetDeletedLogMessage(rowsDeleted));
    }
}
