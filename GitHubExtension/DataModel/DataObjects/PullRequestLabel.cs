// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Dapper;
using Dapper.Contrib.Extensions;
using Serilog;

namespace GitHubExtension.DataModel.DataObjects;

[Table("PullRequestLabel")]
public class PullRequestLabel
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", $"DataModel/{nameof(PullRequestLabel)}"));

    private static readonly ILogger _log = _logger.Value;

    [Key]
    public long Id { get; set; } = DataStore.NoForeignKey;

    // PullRequest table
    public long PullRequest { get; set; } = DataStore.NoForeignKey;

    // Label table
    public long Label { get; set; } = DataStore.NoForeignKey;

    private static PullRequestLabel GetByPullRequestIdAndLabelId(DataStore dataStore, long pullId, long labelId)
    {
        var sql = @"SELECT * FROM PullRequestLabel WHERE PullRequest = @PullId AND Label = @LabelId;";
        var param = new
        {
            PullId = pullId,
            LabelId = labelId,
        };
        return dataStore.Connection!.QueryFirstOrDefault<PullRequestLabel>(sql, param, null);
    }

    public static PullRequestLabel AddLabelToPullRequest(DataStore dataStore, PullRequest pull, Label label)
    {
        var exists = GetByPullRequestIdAndLabelId(dataStore, pull.Id, label.Id);
        if (exists is not null)
        {
            // Already an association between this label and this pull request.
            return exists;
        }

        var newPullLabel = new PullRequestLabel
        {
            PullRequest = pull.Id,
            Label = label.Id,
        };
        newPullLabel.Id = dataStore.Connection!.Insert(newPullLabel);
        return newPullLabel;
    }

    public static IEnumerable<Label> GetLabelsForPullRequest(DataStore dataStore, PullRequest pull)
    {
        var sql = @"SELECT * FROM Label AS L WHERE L.Id IN (SELECT Label FROM PullRequestLabel WHERE PullRequestLabel.PullRequest = @PullId)";
        var param = new
        {
            PullId = pull.Id,
        };

        _log.Verbose(DataStore.GetSqlLogMessage(sql, param));
        return dataStore.Connection!.Query<Label>(sql, param, null) ?? Enumerable.Empty<Label>();
    }

    public static void DeletePullRequestLabelsForPullRequest(DataStore dataStore, PullRequest pullRequest)
    {
        // Delete all PullRequestLabel entries that match this PullRequest Id.
        var sql = @"DELETE FROM PullRequestLabel WHERE PullRequest = $PullRequestId;";
        var command = dataStore.Connection!.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$PullRequestId", pullRequest.Id);
        command.ExecuteNonQuery();
    }
}
