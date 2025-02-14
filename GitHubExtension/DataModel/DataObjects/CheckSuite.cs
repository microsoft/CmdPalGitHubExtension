// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Dapper;
using Dapper.Contrib.Extensions;
using Octokit;
using Serilog;

namespace GitHubExtension.DataModel;

[Table("CheckSuite")]
public class CheckSuite
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", $"DataModel/{nameof(CheckSuite)}"));

    private static readonly ILogger _log = _logger.Value;

    [Key]
    public long Id { get; set; } = DataStore.NoForeignKey;

    public long InternalId { get; set; } = DataStore.NoForeignKey;

    public string Name { get; set; } = string.Empty;

    public long ConclusionId { get; set; } = DataStore.NoForeignKey;

    public long StatusId { get; set; } = DataStore.NoForeignKey;

    public string HtmlUrl { get; set; } = string.Empty;

    public string HeadSha { get; set; } = string.Empty;

    [Write(false)]
    [Computed]
    public CheckConclusion Conclusion => (CheckConclusion)ConclusionId;

    [Write(false)]
    [Computed]
    public CheckStatus Status => (CheckStatus)StatusId;

    [Write(false)]
    [Computed]
    public bool Completed => Status == CheckStatus.Completed;

    private static CheckSuite Create(Octokit.CheckSuite octoCheckSuite)
    {
        var checkSuite = new CheckSuite
        {
            InternalId = octoCheckSuite.Id,
            HeadSha = octoCheckSuite.HeadSha ?? string.Empty,
            Name = octoCheckSuite.App.Name ?? string.Empty,
            HtmlUrl = octoCheckSuite.Url ?? string.Empty,
        };

        try
        {
            // Workaround for bug in Octokit where it can throw here if Conclusion is null.
            checkSuite.ConclusionId = (long)GetCheckConclusion(octoCheckSuite.Conclusion ?? null);
        }
        catch
        {
            checkSuite.ConclusionId = (long)CheckConclusion.None;
        }

        checkSuite.StatusId = (long)GetCheckStatus(octoCheckSuite.Status);

        return checkSuite;
    }

    private static CheckSuite AddOrUpdate(DataStore dataStore, CheckSuite checkSuite)
    {
        // Check for existing entry.
        var existing = GetByInternalId(dataStore, checkSuite.InternalId);
        if (existing is not null)
        {
            // CheckSuites are not heavily updated data, and a simple check against
            // the StatusId and Result will tell us if it needs to be updated.
            // If either state changes, we must update the CheckSuite.
            if ((checkSuite.StatusId != existing.StatusId) || (checkSuite.ConclusionId != existing.ConclusionId))
            {
                checkSuite.Id = existing.Id;
                dataStore.Connection!.Update(checkSuite);
                return checkSuite;
            }
            else
            {
                return checkSuite;
            }
        }

        // No existing pull request, add it.
        checkSuite.Id = dataStore.Connection!.Insert(checkSuite);
        return checkSuite;
    }

    private static CheckConclusion GetCheckConclusion(StringEnum<Octokit.CheckConclusion>? octoCheckConclusion)
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

    private static CheckStatus GetCheckStatus(StringEnum<Octokit.CheckStatus> octoCheckStatus)
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

    public static CheckSuite? GetById(DataStore dataStore, long id)
    {
        return dataStore.Connection!.Get<CheckSuite>(id);
    }

    public static CheckSuite? GetByInternalId(DataStore dataStore, long internalId)
    {
        var sql = @"SELECT * FROM CheckSuite WHERE InternalId = @InternalId;";
        var param = new
        {
            InternalId = internalId,
        };

        return dataStore.Connection!.QueryFirstOrDefault<CheckSuite>(sql, param, null);
    }

    public static CheckSuite GetOrCreateByOctokitCheckSuite(DataStore dataStore, Octokit.CheckSuite checkSuite)
    {
        var newCheckSuite = Create(checkSuite);
        return AddOrUpdate(dataStore, newCheckSuite);
    }

    // It is possible to get the all data about checks from the following collection.
    // It is more efficient to use database queries, so additional method wrappers of queries follow.
    public static IEnumerable<CheckSuite> GetForPullRequest(DataStore dataStore, PullRequest pullRequest)
    {
        var sql = @"SELECT * FROM CheckSuite WHERE HeadSha = @HeadSha;";
        var param = new
        {
            pullRequest.HeadSha,
        };

        return dataStore.Connection!.Query<CheckSuite>(sql, param, null) ?? Enumerable.Empty<CheckSuite>();
    }

    // Composite status for the pull request, taking the minimum result among all runs.
    // If any run is in progress or queued then that will be the current state of the pull request checks.
    // For a Pull Requests Checks to be considered "Completed" it must have every run in the "Completed" state.
    public static CheckStatus GetCheckStatusForPullRequest(DataStore datastore, PullRequest pullRequest)
    {
        var sql = @"SELECT MIN(StatusId) FROM CheckSuite WHERE HeadSha = @HeadSha;";
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
    public static CheckConclusion GetCheckConclusionForPullRequest(DataStore datastore, PullRequest pullRequest)
    {
        // Min conclusion will only give a reasonable result if we also filter by Completed status.
        // Runs that have not started or are in progress would have a conclusion of "None" otherwise, which is
        // lower than failure.
        var sql = @"SELECT MIN(ConclusionId) FROM CheckSuite WHERE HeadSha = @HeadSha AND StatusId = @StatusId;";
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

    public static void DeleteAllForPullRequest(DataStore dataStore, PullRequest pullRequest)
    {
        // Delete all checkruns associated with this pull request.
        var sql = @"DELETE FROM CheckSuite WHERE HeadSha = $HeadSha";
        var command = dataStore.Connection!.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$HeadSha", pullRequest.HeadSha);
        _log.Verbose(DataStore.GetCommandLogMessage(sql, command));
        var rowsDeleted = command.ExecuteNonQuery();
        _log.Verbose(DataStore.GetDeletedLogMessage(rowsDeleted));
    }

    public static void DeleteUnreferenced(DataStore dataStore)
    {
        // Delete any checkruns that have no matching HeadSha in the PullRequest table.
        var sql = @"DELETE FROM CheckSuite WHERE HeadSha NOT IN (SELECT HeadSha FROM PullRequest)";
        var command = dataStore.Connection!.CreateCommand();
        command.CommandText = sql;
        _log.Verbose(DataStore.GetCommandLogMessage(sql, command));
        command.ExecuteNonQuery();
    }
}
