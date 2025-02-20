// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Dapper;
using Dapper.Contrib.Extensions;
using GitHubExtension.Helpers;
using Serilog;

namespace GitHubExtension.DataModel;

[Table("SearchPullRequest")]
public class SearchPullRequest
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", $"DataModel/{nameof(SearchPullRequest)}"));

    private static readonly ILogger _log = _logger.Value;

    [Key]
    public long Id { get; set; } = DataStore.NoForeignKey;

    public long TimeUpdated { get; set; } = DataStore.NoForeignKey;

    // Pull request table
    public long PullRequest { get; set; } = DataStore.NoForeignKey;

    // Search table
    public long Search { get; set; } = DataStore.NoForeignKey;

    [Write(false)]
    [Computed]
    public DateTime UpdatedAt => TimeUpdated.ToDateTime();

    private static SearchPullRequest GetByPullRequestIdAndSearchId(DataStore dataStore, long pullRequestId, long searchId)
    {
        var sql = @"SELECT * FROM SearchPullRequest WHERE PullRequest = @PullRequestId AND Search = @SearchId;";
        var param = new
        {
            PullRequestId = pullRequestId,
            SearchId = searchId,
        };
        return dataStore.Connection!.QueryFirstOrDefault<SearchPullRequest>(sql, param, null);
    }

    public static SearchPullRequest AddPullRequestToSearch(DataStore dataStore, PullRequest pullRequest, Search search)
    {
        var exists = GetByPullRequestIdAndSearchId(dataStore, pullRequest.Id, search.Id);
        if (exists is not null)
        {
            // Update the timestamp for this record so we know it is fresh.
            exists.TimeUpdated = DateTime.Now.ToDataStoreInteger();
            dataStore.Connection!.Update(exists);
            return exists;
        }

        var newSearchPullRequest = new SearchPullRequest
        {
            PullRequest = pullRequest.Id,
            Search = search.Id,
            TimeUpdated = DateTime.Now.ToDataStoreInteger(),
        };
        dataStore.Connection!.Insert(newSearchPullRequest);
        return newSearchPullRequest;
    }

    public static void DeleteUnreferenced(DataStore dataStore)
    {
        var sql = @"DELETE FROM SearchPullRequest WHERE (Search NOT IN (SELECT Id FROM Search)) OR (PullRequest NOT IN (SELECT Id FROM PullRequest))";
        dataStore.Connection!.Execute(sql);
    }

    public static void DeleteBefore(DataStore dataStore, Search search, DateTime date)
    {
        // Delete out of date entries for a given search.
        var sql = @"DELETE FROM SearchPullRequest WHERE TimeUpdated < $Time AND Search = $SearchId;";
        var command = dataStore.Connection!.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$Time", date.ToDataStoreInteger());
        command.Parameters.AddWithValue("$SearchId", search.Id);
        _log.Verbose(DataStore.GetCommandLogMessage(sql, command));
        var rowsDeleted = command.ExecuteNonQuery();
        _log.Verbose(DataStore.GetDeletedLogMessage(rowsDeleted));
    }
}
