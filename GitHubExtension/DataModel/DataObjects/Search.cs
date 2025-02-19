// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Dapper;
using Dapper.Contrib.Extensions;
using GitHubExtension.DataModel.Enums;
using GitHubExtension.Helpers;
using Serilog;

namespace GitHubExtension.DataModel;

[Table("Search")]
public class Search
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", $"DataModel/{nameof(Search)}"));

    private static readonly ILogger _log = _logger.Value;

    // This is the time between seeing a search and updating it's TimeUpdated.
    private static readonly long _updateThreshold = TimeSpan.FromMinutes(2).Ticks;

    [Key]
    public long Id { get; set; } = DataStore.NoForeignKey;

    public string SearchString { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public long TypeId { get; set; } = DataStore.NoForeignKey;

    public long TimeUpdated { get; set; } = DataStore.NoForeignKey;

    public override string ToString() => SearchString;

    [Write(false)]
    private DataStore? DataStore
    {
        get; set;
    }

    [Write(false)]
    [Computed]
    public DateTime UpdatedAt => TimeUpdated.ToDateTime();

    private static Search Create(DataStore dataStore, string name, string searchString, SearchType type)
    {
        return new Search
        {
            DataStore = dataStore,
            Name = name,
            SearchString = searchString,
            TypeId = (long)type,
            TimeUpdated = DateTime.Now.ToDataStoreInteger(),
        };
    }

    private static Search AddOrUpdate(DataStore dataStore, Search search)
    {
        var existing = Get(dataStore, search.Name, search.SearchString, (SearchType)search.TypeId);
        if (existing is not null)
        {
            // The Search time updated is for identifying stale data for deletion later.
            // If it's been recently updated, don't repeatedly update it for every item in a search.
            if ((search.TimeUpdated - existing.TimeUpdated) > _updateThreshold)
            {
                search.Id = existing.Id;
                dataStore.Connection!.Update(search);
                return search;
            }
            else
            {
                return existing;
            }
        }

        // No existing search, add it.
        search.Id = dataStore.Connection!.Insert(search);
        return search;
    }

    public static Search? Get(DataStore dataStore, long id)
    {
        return dataStore.Connection!.Get<Search>(id);
    }

    public static Search? Get(DataStore dataStore, string name, string searchString, SearchType type)
    {
        var sql = @"SELECT * FROM Search WHERE SearchString = @SearchString AND Name = @Name AND TypeId = @TypeId;";
        var param = new
        {
            SearchString = searchString,
            Name = name,
            TypeId = (long)type,
        };

        var search = dataStore.Connection!.QueryFirstOrDefault<Search>(sql, param, null);

        if (search != null)
        {
            search.DataStore = dataStore;
        }

        return search;
    }

    public static Search GetOrCreate(DataStore dataStore, string name, string searchString, SearchType type)
    {
        var newSearch = Create(dataStore, name, searchString, type);
        return AddOrUpdate(dataStore, newSearch);
    }

    [Write(false)]
    [Computed]
    public IEnumerable<Repository> Repositories
    {
        get
        {
            if (DataStore is null)
            {
                return Enumerable.Empty<Repository>();
            }
            else
            {
                return Repository.GetAllForSearch(DataStore, this) ?? Enumerable.Empty<Repository>();
            }
        }
    }

    [Write(false)]
    [Computed]
    public IEnumerable<Issue> Issues
    {
        get
        {
            if (DataStore is null)
            {
                return Enumerable.Empty<Issue>();
            }
            else
            {
                return Issue.GetForSearch(DataStore, this) ?? Enumerable.Empty<Issue>();
            }
        }
    }

    public static void DeleteBefore(DataStore dataStore, DateTime date)
    {
        // Delete search queries older than the date listed.
        var sql = @"DELETE FROM Search WHERE TimeUpdated < $Time;";
        var command = dataStore.Connection!.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$Time", date.ToDataStoreInteger());
        _log.Verbose(DataStore.GetCommandLogMessage(sql, command));
        var rowsDeleted = command.ExecuteNonQuery();
        _log.Verbose(DataStore.GetDeletedLogMessage(rowsDeleted));
    }
}
