// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Dapper;
using Dapper.Contrib.Extensions;
using GitHubExtension.Helpers;
using Serilog;

namespace GitHubExtension.DataModel.DataObjects;

[Table("SearchRepository")]
public class SearchRepository
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", $"DataModel/{nameof(SearchIssue)}"));

    private static readonly ILogger _log = _logger.Value;

    [Key]
    public long Id { get; set; } = DataStore.NoForeignKey;

    public long TimeUpdated { get; set; } = DataStore.NoForeignKey;

    // Repository table
    public long Repository { get; set; } = DataStore.NoForeignKey;

    // Search table
    public long Search { get; set; } = DataStore.NoForeignKey;

    [Write(false)]
    [Computed]
    public DateTime UpdatedAt => TimeUpdated.ToDateTime();

    private static SearchRepository GetByRepositoryIdAndSearchId(DataStore dataStore, long repositoryId, long searchId)
    {
        var sql = @"SELECT * FROM SearchRepository WHERE Repository = @RepositoryId AND Search = @SearchId;";
        var param = new
        {
            RepositoryId = repositoryId,
            SearchId = searchId,
        };
        return dataStore.Connection!.QueryFirstOrDefault<SearchRepository>(sql, param, null);
    }

    public static SearchRepository AddRepositoryToSearch(DataStore dataStore, Repository repository, Search search)
    {
        var exists = GetByRepositoryIdAndSearchId(dataStore, repository.Id, search.Id);
        if (exists is not null)
        {
            // Update the timestamp for this record so we know it is fresh.
            exists.TimeUpdated = DateTime.Now.ToDataStoreInteger();
            dataStore.Connection!.Update(exists);
            return exists;
        }

        var newSearchRepository = new SearchRepository
        {
            Repository = repository.Id,
            Search = search.Id,
            TimeUpdated = DateTime.Now.ToDataStoreInteger(),
        };
        dataStore.Connection!.Insert(newSearchRepository);
        return newSearchRepository;
    }

    public static void DeleteUnreferenced(DataStore dataStore)
    {
        var sql = @"DELETE FROM SearchRepository WHERE Search NOT IN (SELECT Id FROM Search) OR (Repository NOT IN (Select Id FROM Repository));";
        var command = dataStore.Connection!.CreateCommand();
        command.CommandText = sql;
        _log.Verbose(DataStore.GetCommandLogMessage(sql, command));
        var rowsDeleted = command.ExecuteNonQuery();
        _log.Verbose(DataStore.GetDeletedLogMessage(rowsDeleted));
    }

    public static void DeleteBefore(DataStore dataStore, Search search, DateTime date)
    {
        // Delete search queries older than the date listed.
        var sql = @"DELETE FROM SearchRepository WHERE TimeUpdated < $Time AND Search = $SearchId;";
        var command = dataStore.Connection!.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$Time", date.ToDataStoreInteger());
        command.Parameters.AddWithValue("$SearchId", search.Id);
        _log.Verbose(DataStore.GetCommandLogMessage(sql, command));
        var rowsDeleted = command.ExecuteNonQuery();
        _log.Verbose(DataStore.GetDeletedLogMessage(rowsDeleted));
    }
}
