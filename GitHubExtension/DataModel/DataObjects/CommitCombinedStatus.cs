// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Dapper;
using Dapper.Contrib.Extensions;
using Serilog;

namespace GitHubExtension.DataModel;

[Table("CommitCombinedStatus")]
public class CommitCombinedStatus
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", $"DataModel/{nameof(CommitCombinedStatus)}"));

    private static readonly ILogger _log = _logger.Value;

    [Key]
    public long Id { get; set; } = DataStore.NoForeignKey;

    public long StateId { get; set; } = DataStore.NoForeignKey;

    public string HeadSha { get; set; } = string.Empty;

    [Write(false)]
    [Computed]
    public CommitState State => (CommitState)StateId;

    public static CommitCombinedStatus Create(Octokit.CombinedCommitStatus octoStatus)
    {
        return new CommitCombinedStatus
        {
            HeadSha = octoStatus.Sha,
            StateId = (long)(octoStatus.TotalCount == 0 ? CommitState.None : GetCommitState(octoStatus.State)),
        };
    }

    public static CommitCombinedStatus AddOrUpdate(DataStore dataStore, CommitCombinedStatus status)
    {
        // Check for existing entry.
        var existing = GetByHeadSha(dataStore, status.HeadSha);
        if (existing is not null)
        {
            // CheckRuns are not heavily updated data, and a simple check against
            // the StatusId and Result will tell us if it needs to be updated.
            // If either state changes, we must update the CheckRun.
            if (existing.StateId != status.StateId)
            {
                existing.StateId = status.StateId;
                dataStore.Connection!.Update(existing);
                return existing;
            }
            else
            {
                return existing;
            }
        }

        // No existing combined commit status, add it.
        status.Id = dataStore.Connection!.Insert(status);
        return status;
    }

    public static CommitCombinedStatus GetOrCreate(DataStore dataStore, Octokit.CombinedCommitStatus status)
    {
        var newStatus = Create(status);
        return AddOrUpdate(dataStore, newStatus);
    }

    private static CommitState GetCommitState(Octokit.StringEnum<Octokit.CommitState> octoCommitState)
    {
        CommitState state;
        try
        {
            state = Enum.Parse<CommitState>(octoCommitState.Value.ToString());
        }
        catch (Exception)
        {
            // This error means a programming error or Octokit added to or changed their enum.
            _log.Error($"Found Unknown CheckStatus value: {octoCommitState.Value}");
            return CommitState.Unknown;
        }

        return state;
    }

    public static CommitCombinedStatus? GetByHeadSha(DataStore dataStore, string headSha)
    {
        var sql = @"SELECT * FROM CommitCombinedStatus WHERE HeadSha = @HeadSha;";
        var param = new
        {
            HeadSha = headSha,
        };

        return dataStore.Connection!.QueryFirstOrDefault<CommitCombinedStatus>(sql, param, null);
    }

    public static CommitState GetCommitState(DataStore dataStore, PullRequest pullRequest)
    {
        var sql = @"SELECT * FROM CommitCombinedStatus WHERE HeadSha = @HeadSha;";
        var param = new
        {
            pullRequest.HeadSha,
        };

        var status = dataStore.Connection!.QueryFirstOrDefault<CommitCombinedStatus>(sql, param, null);
        if (status == null)
        {
            return CommitState.Unknown;
        }

        return status.State;
    }

    public static void DeleteUnreferenced(DataStore dataStore)
    {
        // Delete any checkruns that have no matching HeadSha in the PullRequest table.
        var sql = @"DELETE FROM CommitCombinedStatus WHERE HeadSha NOT IN (SELECT HeadSha FROM PullRequest)";
        var command = dataStore.Connection!.CreateCommand();
        command.CommandText = sql;
        _log.Verbose(DataStore.GetCommandLogMessage(sql, command));
        var rowsDeleted = command.ExecuteNonQuery();
        _log.Verbose(DataStore.GetDeletedLogMessage(rowsDeleted));
    }

    public static void DeleteForPullRequest(DataStore dataStore, PullRequest pullRequest)
    {
        // Delete all checkruns associated with this pull request.
        var sql = @"DELETE FROM CommitCombinedStatus WHERE HeadSha = $HeadSha";
        var command = dataStore.Connection!.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$HeadSha", pullRequest.HeadSha);
        _log.Verbose(DataStore.GetCommandLogMessage(sql, command));
        var rowsDeleted = command.ExecuteNonQuery();
        _log.Verbose(DataStore.GetDeletedLogMessage(rowsDeleted));
    }
}
