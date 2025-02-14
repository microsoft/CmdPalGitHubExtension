// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Dapper;
using Dapper.Contrib.Extensions;
using Serilog;

namespace GitHubExtension.DataModel;

[Table("PullRequestAssign")]
public class PullRequestAssign
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", $"DataModel/{nameof(PullRequestAssign)}"));

    private static readonly ILogger _log = _logger.Value;

    [Key]
    public long Id { get; set; } = DataStore.NoForeignKey;

    // PullRequest table
    public long PullRequest { get; set; } = DataStore.NoForeignKey;

    // User table
    public long User { get; set; } = DataStore.NoForeignKey;

    private static PullRequestAssign GetByPullRequestIdAndUserId(DataStore dataStore, long pullId, long userId)
    {
        var sql = @"SELECT * FROM PullRequestAssign WHERE PullRequest = @PullId AND User = @UserId;";
        var param = new
        {
            PullId = pullId,
            UserId = userId,
        };

        return dataStore.Connection!.QueryFirstOrDefault<PullRequestAssign>(sql, param, null);
    }

    public static PullRequestAssign AddUserToPullRequest(DataStore dataStore, PullRequest pull, User user)
    {
        var exists = GetByPullRequestIdAndUserId(dataStore, pull.Id, user.Id);
        if (exists is not null)
        {
            // Already an association between this label and this issue.
            return exists;
        }

        var newPullRequestAssign = new PullRequestAssign
        {
            PullRequest = pull.Id,
            User = user.Id,
        };

        newPullRequestAssign.Id = dataStore.Connection!.Insert(newPullRequestAssign);
        return newPullRequestAssign;
    }

    public static IEnumerable<User> GetUsersForPullRequest(DataStore dataStore, PullRequest pull)
    {
        var sql = @"SELECT * FROM User AS U WHERE U.Id IN (SELECT User FROM PullRequestAssign WHERE PullRequestAssign.PullRequest = @PullId)";
        var param = new
        {
            PullId = pull.Id,
        };

        _log.Verbose(DataStore.GetSqlLogMessage(sql, param));
        return dataStore.Connection!.Query<User>(sql, param, null) ?? Enumerable.Empty<User>();
    }

    public static void DeletePullRequestAssignForPullRequest(DataStore dataStore, PullRequest pullRequest)
    {
        // Delete all PullRequestAssign entries that match this PullRequest Id.
        var sql = @"DELETE FROM PullRequestAssign WHERE PullRequest = $PullRequestId;";
        var command = dataStore.Connection!.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$PullRequestId", pullRequest.Id);
        command.ExecuteNonQuery();
    }
}
