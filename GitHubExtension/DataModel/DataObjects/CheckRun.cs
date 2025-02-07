// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Dapper;
using Dapper.Contrib.Extensions;
using GitHubExtension.Helpers;
using Octokit;
using Serilog;

namespace GitHubExtension.DataModel;

[Table("CheckRun")]
public class CheckRun
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", $"DataModel/{nameof(CheckRun)}"));

    private static readonly ILogger _log = _logger.Value;

    [Key]
    public long Id { get; set; } = DataStore.NoForeignKey;

    public long InternalId { get; set; } = DataStore.NoForeignKey;

    public string Name { get; set; } = string.Empty;

    public long ConclusionId { get; set; } = DataStore.NoForeignKey;

    public long StatusId { get; set; } = DataStore.NoForeignKey;

    public string Result { get; set; } = string.Empty;

    public string DetailsUrl { get; set; } = string.Empty;

    public string HtmlUrl { get; set; } = string.Empty;

    public string HeadSha { get; set; } = string.Empty;

    public long TimeStarted { get; set; } = DataStore.NoForeignKey;

    public long TimeCompleted { get; set; } = DataStore.NoForeignKey;

    [Write(false)]
    [Computed]
    public DateTime StartedAt => TimeStarted.ToDateTime();

    [Write(false)]
    [Computed]
    public DateTime CompletedAt => TimeCompleted.ToDateTime();

    [Write(false)]
    [Computed]
    public CheckConclusion Conclusion => (CheckConclusion)ConclusionId;

    [Write(false)]
    [Computed]
    public CheckStatus Status => (CheckStatus)StatusId;

    [Write(false)]
    [Computed]
    public bool Completed => Status == CheckStatus.Completed;

    public override string ToString() => $"[{Status}][{Conclusion}] {Name}";

    private static CheckRun CreateFromOctokitCheckRun(Octokit.CheckRun octoCheckRun)
    {
        var checkRun = new CheckRun
        {
            InternalId = octoCheckRun.Id,
            HeadSha = octoCheckRun.HeadSha ?? string.Empty,
            Name = octoCheckRun.Name ?? string.Empty,
            Result = octoCheckRun.Output.Summary ?? string.Empty,
            DetailsUrl = octoCheckRun.DetailsUrl ?? string.Empty,
            HtmlUrl = octoCheckRun.HtmlUrl ?? string.Empty,
            TimeStarted = octoCheckRun.StartedAt.DateTime.ToDataStoreInteger(),
            TimeCompleted = octoCheckRun.CompletedAt.HasValue ? octoCheckRun.CompletedAt.Value.DateTime.ToDataStoreInteger() : 0,
        };

        try
        {
            // Workaround for bug in Octokit where it can throw here if Conclusion is null.
            checkRun.ConclusionId = (long)GetCheckRunConclusion(octoCheckRun.Conclusion ?? null);
        }
        catch
        {
            checkRun.ConclusionId = (long)CheckConclusion.None;
        }

        checkRun.StatusId = (long)GetCheckRunStatus(octoCheckRun.Status);

        return checkRun;
    }

    private static CheckRun AddOrUpdateCheckRun(DataStore dataStore, CheckRun checkRun)
    {
        // Check for existing entry.
        var existing = GetByInternalId(dataStore, checkRun.InternalId);
        if (existing is not null)
        {
            // CheckRuns are not heavily updated data, and a simple check against
            // the StatusId and Result will tell us if it needs to be updated.
            // If either state changes, we must update the CheckRun.
            if ((checkRun.StatusId != existing.StatusId) || (checkRun.ConclusionId != existing.ConclusionId))
            {
                checkRun.Id = existing.Id;
                dataStore.Connection!.Update(checkRun);
                return checkRun;
            }
            else
            {
                return existing;
            }
        }

        // No existing pull request, add it.
        checkRun.Id = dataStore.Connection!.Insert(checkRun);
        return checkRun;
    }

    private static CheckConclusion GetCheckRunConclusion(StringEnum<Octokit.CheckConclusion>? octoCheckConclusion)
    {
        if (octoCheckConclusion is null || !octoCheckConclusion.HasValue)
        {
            return CheckConclusion.None;
        }

        CheckConclusion conclusion;
        try
        {
            // We match all the same conclusion enum values Octokit does.
            // If we cannot match, it may be due to an API change or some other error,
            // in which case we set the conclusion to Unknown.
            conclusion = Enum.Parse<CheckConclusion>(octoCheckConclusion.Value.Value.ToString());
        }
        catch (Exception)
        {
            // This error means a programming error or Octokit added to or changed their enum.
            _log.Error($"Found Unknown CheckConclusion value: {octoCheckConclusion.Value.Value}");
            return CheckConclusion.Unknown;
        }

        return conclusion;
    }

    private static CheckStatus GetCheckRunStatus(StringEnum<Octokit.CheckStatus> octoCheckStatus)
    {
        CheckStatus status;
        try
        {
            // We match all the same conclusion enum values Octokit does.
            // If we cannot match, it may be due to an API change or some other error,
            // in which case we set the conclusion to Unknown.
            status = Enum.Parse<CheckStatus>(octoCheckStatus.Value.ToString());
        }
        catch (Exception)
        {
            // This error means a programming error or Octokit added to or changed their enum.
            _log.Error($"Found Unknown CheckStatus value: {octoCheckStatus.Value}");
            return CheckStatus.Unknown;
        }

        return status;
    }

    public static CheckRun? GetById(DataStore dataStore, long id)
    {
        return dataStore.Connection!.Get<CheckRun>(id);
    }

    public static CheckRun? GetByInternalId(DataStore dataStore, long internalId)
    {
        var sql = @"SELECT * FROM CheckRun WHERE InternalId = @InternalId;";
        var param = new
        {
            InternalId = internalId,
        };

        return dataStore.Connection!.QueryFirstOrDefault<CheckRun>(sql, param, null);
    }

    public static CheckRun GetOrCreateByOctokitCheckRun(DataStore dataStore, Octokit.CheckRun checkRun)
    {
        var newCheckRun = CreateFromOctokitCheckRun(checkRun);
        return AddOrUpdateCheckRun(dataStore, newCheckRun);
    }

    // It is possible to get the all data about checks from the following collection.
    // It is more efficient to use database queries, so additional method wrappers of queries follow.
    public static IEnumerable<CheckRun> GetCheckRunsForPullRequest(DataStore dataStore, PullRequest pullRequest)
    {
        var sql = @"SELECT * FROM CheckRun WHERE HeadSha = @HeadSha;";
        var param = new
        {
            pullRequest.HeadSha,
        };

        return dataStore.Connection!.Query<CheckRun>(sql, param, null) ?? Enumerable.Empty<CheckRun>();
    }

    // If a failure is detected, the user and UI will probably want to display it. It could display all
    // failures with the below query, ordered by severity of failure and then by time completed.
    public static IEnumerable<CheckRun> GetFailedCheckRunsForPullRequest(DataStore dataStore, PullRequest pullRequest)
    {
        // This gets all failure results, ordering them by most severe first, then by time completed.
        // This gets the first failure as the first item in the list.
        var sql = @"SELECT * FROM CheckRun WHERE HeadSha = @HeadSha AND (ConclusionId >= @MinConclusion) AND (ConclusionId <= @MaxConclusion) ORDER BY ConclusionId, TimeCompleted;";
        var param = new
        {
            pullRequest.HeadSha,
            MinConclusion = (long)CheckConclusion.Failure,
            MaxConclusion = (long)CheckConclusion.ActionRequired,
        };

        _log.Verbose(DataStore.GetSqlLogMessage(sql, param));
        return dataStore.Connection!.Query<CheckRun>(sql, param, null) ?? Enumerable.Empty<CheckRun>();
    }

    // Composite status for the pull request, taking the minimum result among all runs.
    // If any run is in progress or queued then that will be the current state of the pull request checks.
    // For a Pull Requests Checks to be considered "Completed" it must have every run in the "Completed" state.
    public static CheckStatus GetCheckRunStatusForPullRequest(DataStore datastore, PullRequest pullRequest)
    {
        var sql = @"SELECT MIN(StatusId) FROM CheckRun WHERE HeadSha = @HeadSha;";
        var param = new
        {
            pullRequest.HeadSha,
        };

        _log.Verbose(DataStore.GetSqlLogMessage(sql, param));

        // Query results in NULL if there are no entries, and QueryFirstOrDefault will throw trying
        // to assign null to an integer. In this instance we catch it and return the None type.
        try
        {
            return (CheckStatus)datastore.Connection!.QueryFirstOrDefault<long>(sql, param);
        }
        catch
        {
            return CheckStatus.None;
        }
    }

    // Composite conclusion for the pull request, taking the minimum result among all runs.
    // If any completed run has failed or cancelled then that will show up here and consider
    // the entire pull request as having that state. This means for a set of completed checks for
    // for a pull request to be considered "Success", all runs must be "Success" or "Skipped".
    public static CheckConclusion GetCheckRunConclusionForPullRequest(DataStore datastore, PullRequest pullRequest)
    {
        // Min conclusion will only give a reasonable result if we also filter by Completed status.
        // Runs that have not started or are in progress would have a conclusion of "None" otherwise, which is
        // lower than failure.
        var sql = @"SELECT MIN(ConclusionId) FROM CheckRun WHERE HeadSha = @HeadSha AND StatusId = @StatusId;";
        var param = new
        {
            pullRequest.HeadSha,
            StatusId = (long)CheckStatus.Completed,
        };

        _log.Verbose(DataStore.GetSqlLogMessage(sql, param));

        // Query results in NULL if there are no entries, and QueryFirstOrDefault will throw trying
        // to assign null to an integer. In this instance we catch it and return the None type.
        try
        {
            return (CheckConclusion)datastore.Connection!.QueryFirstOrDefault<long>(sql, param);
        }
        catch
        {
            return CheckConclusion.None;
        }
    }

    public static void DeleteUnreferenced(DataStore dataStore)
    {
        // Delete any checkruns that have no matching HeadSha in the PullRequest table.
        var sql = @"DELETE FROM CheckRun WHERE HeadSha NOT IN (SELECT HeadSha FROM PullRequest)";
        var command = dataStore.Connection!.CreateCommand();
        command.CommandText = sql;
        _log.Verbose(DataStore.GetCommandLogMessage(sql, command));
        var rowsDeleted = command.ExecuteNonQuery();
        _log.Verbose(DataStore.GetDeletedLogMessage(rowsDeleted));
    }

    public static void DeleteAllForPullRequest(DataStore dataStore, PullRequest pullRequest)
    {
        // Delete all checkruns associated with this pull request.
        var sql = @"DELETE FROM CheckRun WHERE HeadSha = $HeadSha";
        var command = dataStore.Connection!.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$HeadSha", pullRequest.HeadSha);
        _log.Verbose(DataStore.GetCommandLogMessage(sql, command));
        var rowsDeleted = command.ExecuteNonQuery();
        _log.Verbose(DataStore.GetDeletedLogMessage(rowsDeleted));
    }
}
