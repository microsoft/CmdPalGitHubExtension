// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Dapper;
using Dapper.Contrib.Extensions;
using GitHubExtension.Helpers;
using Serilog;

namespace GitHubExtension.DataModel;

/// <summary>
/// A snapshot in time of a PullRequest's Checks status and conclusion.
/// </summary>
[Table("PullRequestStatus")]
public class PullRequestStatus
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", $"DataModel/{nameof(PullRequestStatus)}"));

    private static readonly ILogger _log = _logger.Value;

    [Key]
    public long Id { get; set; } = DataStore.NoForeignKey;

    // Pull Request table
    public long PullRequestId { get; set; } = DataStore.NoForeignKey;

    public long ConclusionId { get; set; } = DataStore.NoForeignKey;

    public long StatusId { get; set; } = DataStore.NoForeignKey;

    public long StateId { get; set; } = DataStore.NoForeignKey;

    public string Result { get; set; } = string.Empty;

    public string DetailsUrl { get; set; } = string.Empty;

    public string HtmlUrl { get; set; } = string.Empty;

    public string HeadSha { get; set; } = string.Empty;

    public long TimeOccurred { get; set; } = DataStore.NoForeignKey;

    public long TimeCreated { get; set; } = DataStore.NoForeignKey;

    [Write(false)]
    public DataStore? DataStore { get; set; }

    /// <summary>
    /// Gets the time this PullRequestStatus was created.
    /// </summary>
    [Write(false)]
    [Computed]
    public DateTime CreatedAt => TimeCreated.ToDateTime();

    /// <summary>
    /// Gets the time the CheckRuns for this pull request were completed, updated, or checked.
    /// </summary>
    [Write(false)]
    [Computed]
    public DateTime OccurredAt => TimeOccurred.ToDateTime();

    [Write(false)]
    [Computed]
    public CheckConclusion Conclusion => (CheckConclusion)ConclusionId;

    [Write(false)]
    [Computed]
    public CheckStatus Status => (CheckStatus)StatusId;

    [Write(false)]
    [Computed]
    public CommitState State => (CommitState)StateId;

    [Write(false)]
    [Computed]
    public bool Pending => State == CommitState.Pending || State == CommitState.Unknown || Status == CheckStatus.InProgress;

    [Write(false)]
    [Computed]
    public bool Completed => !Pending && Status == CheckStatus.Completed;

    [Write(false)]
    [Computed]
    public bool Failed => State == CommitState.Failure || State == CommitState.Error || ((Conclusion > CheckConclusion.None) && (Conclusion < CheckConclusion.Neutral));

    [Write(false)]
    [Computed]
    public bool Succeeded => Completed && !Failed && (State == CommitState.Success || State == CommitState.None);

    [Write(false)]
    [Computed]
    public PullRequestCombinedStatus CombinedStatus => GetCombinedStatus();

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

    public override string ToString() => $"[{Status}][{Conclusion}] {PullRequest}";

    public static PullRequestStatus Create(PullRequest pullRequest)
    {
        // Get Current status of this pull request, and create a summary capture.
        var pullRequestStatus = new PullRequestStatus
        {
            PullRequestId = pullRequest.Id,
            ConclusionId = (long)pullRequest.ChecksConclusion,
            StatusId = (long)pullRequest.ChecksStatus,
            HeadSha = pullRequest.HeadSha,
            HtmlUrl = pullRequest.HtmlUrl,
            DetailsUrl = pullRequest.HtmlUrl,
            TimeOccurred = pullRequest.TimeUpdated,
            Result = string.Empty,
        };

        // Get the ConclusionId and StatusId from the CheckSuites.
        // Get the Combined Commit State.
        pullRequestStatus.ConclusionId = (long)pullRequest.CheckSuiteConclusion;
        pullRequestStatus.StatusId = (long)pullRequest.CheckSuiteStatus;
        pullRequestStatus.StateId = (long)pullRequest.CommitState;

        // Update the fields if necessary.
        var failedChecks = pullRequest.FailedChecks;
        if (failedChecks.Any())
        {
            // If we have a failed check, make the first one the Result & DetailsUrl.
            pullRequestStatus.DetailsUrl = failedChecks.First().DetailsUrl;
            pullRequestStatus.Result = failedChecks.First().Result;
            pullRequestStatus.TimeOccurred = failedChecks.First().TimeCompleted == 0 ? DateTime.Now.ToDataStoreInteger() : failedChecks.First().TimeCompleted;
        }
        else
        {
            // No failed checks, so set the DetailsUrl and Result to be the first Check.
            var checks = pullRequest.Checks;
            if (checks.Any())
            {
                pullRequestStatus.DetailsUrl = checks.First().DetailsUrl;
                pullRequestStatus.Result = checks.First().Result;
                pullRequestStatus.TimeOccurred = checks.First().TimeCompleted == 0 ? DateTime.Now.ToDataStoreInteger() : checks.First().TimeCompleted;
            }
        }

        pullRequestStatus.TimeCreated = DateTime.Now.ToDataStoreInteger();
        return pullRequestStatus;
    }

    public static PullRequestStatus? Get(DataStore dataStore, PullRequest pullRequest)
    {
        var sql = @"SELECT * FROM PullRequestStatus WHERE PullRequestId = @PullRequestId ORDER BY TimeCreated DESC LIMIT 1;";
        var param = new
        {
            PullRequestId = pullRequest.Id,
        };

        _log.Verbose(DataStore.GetSqlLogMessage(sql, param));
        var pullRequestStatus = dataStore.Connection!.QueryFirstOrDefault<PullRequestStatus>(sql, param, null);
        if (pullRequestStatus is not null)
        {
            // Add Datastore so this object can make internal queries.
            pullRequestStatus.DataStore = dataStore;
        }

        return pullRequestStatus;
    }

    public static PullRequestStatus Add(DataStore dataStore, PullRequest pullRequest)
    {
        var pullRequestStatus = Create(pullRequest);
        pullRequestStatus.Id = dataStore.Connection!.Insert(pullRequestStatus);
        _log.Debug($"Inserted PullRequestStatus, Id = {pullRequestStatus.Id}");

        // Remove older records we no longer need.
        DeleteOutdatedForPullRequest(dataStore, pullRequest);

        pullRequestStatus.DataStore = dataStore;
        return pullRequestStatus;
    }

    public static void DeleteOutdatedForPullRequest(DataStore dataStore, PullRequest pullRequest)
    {
        // Delete any records beyond the most recent 2.
        var sql = @"DELETE FROM PullRequestStatus WHERE PullRequestId = $Id AND Id NOT IN (SELECT Id FROM PullRequestStatus WHERE PullRequestId = $Id ORDER BY TimeCreated DESC LIMIT 2)";
        var command = dataStore.Connection!.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$Id", pullRequest.Id);
        _log.Verbose(DataStore.GetCommandLogMessage(sql, command));
        var rowsDeleted = command.ExecuteNonQuery();
        _log.Verbose(DataStore.GetDeletedLogMessage(rowsDeleted));
    }

    public static void DeleteUnreferenced(DataStore dataStore)
    {
        // Delete any where the HeadSha has no match in pull requests, as that means either the pull request
        // no longer exists, or it has pushed new changes and the HeadSha is different, triggering new checks.
        // In either case it is safe to remove the associated PullRequestStatus.
        var sql = @"DELETE FROM PullRequestStatus WHERE HeadSha NOT IN (SELECT HeadSha FROM PullRequest)";
        var command = dataStore.Connection!.CreateCommand();
        command.CommandText = sql;
        _log.Verbose(DataStore.GetCommandLogMessage(sql, command));
        var rowsDeleted = command.ExecuteNonQuery();
        _log.Verbose(DataStore.GetDeletedLogMessage(rowsDeleted));
    }

    private PullRequestCombinedStatus GetCombinedStatus()
    {
        // If any indication of failure, we know it is failed.
        if (Failed)
        {
            return PullRequestCombinedStatus.Failed;
        }

        // If anything is pending, we cannot be in a success state.
        if (Pending)
        {
            return PullRequestCombinedStatus.Pending;
        }

        // If not failed or pending we should be in a success state.
        // But confirm that we are in a state we understand.
        if (Succeeded)
        {
            return PullRequestCombinedStatus.Success;
        }

        _log.Warning($"Unknown PR Status:  State={State} Status={Status} Conclusion={Conclusion}");
        return PullRequestCombinedStatus.Unknown;
    }
}
