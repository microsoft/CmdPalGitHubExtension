// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Dapper;
using Dapper.Contrib.Extensions;
using GitHubExtension.Helpers;
using Serilog;

namespace GitHubExtension.DataModel;

[Table("Review")]
public class Review
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", $"DataModel/{nameof(Review)}"));

    private static readonly ILogger _log = _logger.Value;

    [Key]
    public long Id { get; set; } = DataStore.NoForeignKey;

    public long InternalId { get; set; } = DataStore.NoForeignKey;

    // Pull request table
    public long PullRequestId { get; set; } = DataStore.NoForeignKey;

    // User table
    public long AuthorId { get; set; } = DataStore.NoForeignKey;

    public string Body { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string HtmlUrl { get; set; } = string.Empty;

    public long TimeSubmitted { get; set; } = DataStore.NoForeignKey;

    public long TimeLastObserved { get; set; } = DataStore.NoForeignKey;

    [Write(false)]
    private DataStore? DataStore
    {
        get; set;
    }

    [Write(false)]
    [Computed]
    public DateTime SubmittedAt => TimeSubmitted.ToDateTime();

    [Write(false)]
    [Computed]
    public DateTime LastObservedAt => TimeLastObserved.ToDateTime();

    [Write(false)]
    [Computed]
    public PullRequest PullRequest
    {
        get
        {
            if (DataStore == null)
            {
                return new PullRequest();
            }
            else
            {
                return PullRequest.GetById(DataStore, PullRequestId) ?? new PullRequest();
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

    public override string ToString() => $"{PullRequestId}: {AuthorId} - {State}";

    // Create review from OctoKit review data
    private static Review CreateFromOctokitReview(DataStore dataStore, Octokit.PullRequestReview okitReview, long pullRequestId)
    {
        var review = new Review
        {
            DataStore = dataStore,
            InternalId = okitReview.Id,
            Body = okitReview.Body ?? string.Empty,
            State = okitReview.State.Value.ToString(),
            HtmlUrl = okitReview.HtmlUrl ?? string.Empty,
            TimeSubmitted = okitReview.SubmittedAt.DateTime.ToDataStoreInteger(),
            TimeLastObserved = DateTime.UtcNow.ToDataStoreInteger(),
        };

        // Author is a row id in the User table
        var author = User.GetOrCreateByOctokitUser(dataStore, okitReview.User);
        review.AuthorId = author.Id;

        // Repo is a row id in the Repository table.
        // It is likely the case that we already know the repository id (such as when querying pulls for a repository).
        if (pullRequestId != DataStore.NoForeignKey)
        {
            review.PullRequestId = pullRequestId;
        }

        return review;
    }

    private static Review AddOrUpdateReview(DataStore dataStore, Review review)
    {
        // Check for existing pull request data.
        var existingReview = GetByInternalId(dataStore, review.InternalId);
        if (existingReview is not null)
        {
            // Existing pull requests must always be updated to update the LastObserved time.
            review.Id = existingReview.Id;
            dataStore.Connection!.Update(review);
            review.DataStore = dataStore;
            return review;
        }

        // No existing pull request, add it.
        review.Id = dataStore.Connection!.Insert(review);
        review.DataStore = dataStore;
        return review;
    }

    public static Review? GetById(DataStore dataStore, long id)
    {
        var review = dataStore.Connection!.Get<Review>(id);
        if (review is not null)
        {
            // Add Datastore so this object can make internal queries.
            review.DataStore = dataStore;
        }

        return review;
    }

    public static Review? GetByInternalId(DataStore dataStore, long internalId)
    {
        var sql = @"SELECT * FROM Review WHERE InternalId = @InternalId;";
        var param = new
        {
            InternalId = internalId,
        };

        var review = dataStore.Connection!.QueryFirstOrDefault<Review>(sql, param, null);
        if (review is not null)
        {
            // Add Datastore so this object can make internal queries.
            review.DataStore = dataStore;
        }

        return review;
    }

    public static Review GetOrCreateByOctokitReview(DataStore dataStore, Octokit.PullRequestReview octokitReview, long repositoryId = DataStore.NoForeignKey)
    {
        var newReview = CreateFromOctokitReview(dataStore, octokitReview, repositoryId);
        return AddOrUpdateReview(dataStore, newReview);
    }

    public static IEnumerable<Review> GetAllForPullRequest(DataStore dataStore, PullRequest pullRequest)
    {
        var sql = @"SELECT * FROM Review WHERE PullRequestId = @PullRequestId ORDER BY TimeSubmitted DESC;";
        var param = new
        {
            PullRequestId = pullRequest.Id,
        };

        _log.Verbose(DataStore.GetSqlLogMessage(sql, param));
        var reviews = dataStore.Connection!.Query<Review>(sql, param, null) ?? Enumerable.Empty<Review>();
        foreach (var review in reviews)
        {
            review.DataStore = dataStore;
        }

        return reviews;
    }

    public static IEnumerable<Review> GetAllForUser(DataStore dataStore, User user)
    {
        var sql = @"SELECT * FROM Review WHERE AuthorId = @AuthorId;";
        var param = new
        {
            AuthorId = user.Id,
        };

        _log.Verbose(DataStore.GetSqlLogMessage(sql, param));
        var reviews = dataStore.Connection!.Query<Review>(sql, param, null) ?? Enumerable.Empty<Review>();
        foreach (var review in reviews)
        {
            review.DataStore = dataStore;
        }

        return reviews;
    }

    public static void DeleteUnreferenced(DataStore dataStore)
    {
        // Delete any reviews that have no matching PullRequestId in the PullRequest table.
        var sql = @"DELETE FROM Review WHERE PullRequestId NOT IN (SELECT Id FROM PullRequest)";
        var command = dataStore.Connection!.CreateCommand();
        command.CommandText = sql;
        _log.Verbose(DataStore.GetCommandLogMessage(sql, command));
        var rowsDeleted = command.ExecuteNonQuery();
        _log.Verbose(DataStore.GetDeletedLogMessage(rowsDeleted));
    }
}
