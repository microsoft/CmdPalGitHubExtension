// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Dapper;
using Dapper.Contrib.Extensions;
using GitHubExtension.Helpers;
using Serilog;

namespace GitHubExtension.DataModel;

[Table("SearchUser")]
public class SearchUser
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", $"DataModel/{nameof(SearchUser)}"));

    private static readonly ILogger _log = _logger.Value;

    [Key]
    public long Id { get; set; } = DataStore.NoForeignKey;

    public long TimeUpdated { get; set; } = DataStore.NoForeignKey;

    // User table
    public long User { get; set; } = DataStore.NoForeignKey;

    // Search table
    public long Search { get; set; } = DataStore.NoForeignKey;

    [Write(false)]
    [Computed]
    public DateTime UpdatedAt => TimeUpdated.ToDateTime();

    private static SearchUser GetByUserIdAndSearchId(DataStore dataStore, long userId, long searchId)
    {
        var sql = @"SELECT * FROM SearchUser WHERE User = @UserId AND Search = @SearchId;";
        var param = new
        {
            UserId = userId,
            SearchId = searchId,
        };
        return dataStore.Connection!.QueryFirstOrDefault<SearchUser>(sql, param, null);
    }

    public static SearchUser AddUserToSearch(DataStore dataStore, User user, Search search)
    {
        var exists = GetByUserIdAndSearchId(dataStore, user.Id, search.Id);
        if (exists is not null)
        {
            // Update the timestamp for this record so we know it is fresh.
            exists.TimeUpdated = DateTime.Now.ToDataStoreInteger();
            dataStore.Connection!.Update(exists);
            return exists;
        }

        var newSearchUser = new SearchUser
        {
            User = user.Id,
            Search = search.Id,
            TimeUpdated = DateTime.Now.ToDataStoreInteger(),
        };
        newSearchUser.Id = dataStore.Connection!.Insert(newSearchUser);
        return newSearchUser;
    }

    public static void DeleteUnreferenced(DataStore dataStore)
    {
        // Delete any where the Search Id or User Id does not exist.
        var sql = @"DELETE FROM SearchUser WHERE (Search NOT IN (SELECT Id FROM Search)) OR (User NOT IN (SELECT Id FROM User))";
        var command = dataStore.Connection!.CreateCommand();
        command.CommandText = sql;
        _log.Verbose(DataStore.GetCommandLogMessage(sql, command));
        var rowsDeleted = command.ExecuteNonQuery();
        _log.Verbose(DataStore.GetDeletedLogMessage(rowsDeleted));
    }

    public static void DeleteBefore(DataStore dataStore, Search search, DateTime date)
    {
        // Delete out of date entries for a given search.
        var sql = @"DELETE FROM SearchUser WHERE TimeUpdated < $Time AND Search = $SearchId;";
        var command = dataStore.Connection!.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$Time", date.ToDataStoreInteger());
        command.Parameters.AddWithValue("$SearchId", search.Id);
        _log.Verbose(DataStore.GetCommandLogMessage(sql, command));
        var rowsDeleted = command.ExecuteNonQuery();
        _log.Verbose(DataStore.GetDeletedLogMessage(rowsDeleted));
    }
}
