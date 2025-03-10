// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Dapper;
using Dapper.Contrib.Extensions;
using GitHubExtension.Controls;
using GitHubExtension.DataModel;
using GitHubExtension.DataModel.Enums;
using GitHubExtension.Helpers;
using Octokit;
using Serilog;

namespace GitHubExtension.PersistentData;

[Table("Search")]
public class Search : ISearch
{
    private static readonly Lazy<ILogger> _logger = new(() => Log.ForContext("SourceContext", $"PersistentData/{nameof(Search)}"));

    private static readonly ILogger _log = _logger.Value;

    [Key]
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string SearchString { get; set; } = string.Empty;

    public bool IsTopLevel { get; set; }

    private SearchType _searchType = SearchType.Unkown;

    [Write(false)]
    [Computed]
    public SearchType Type
    {
        get
        {
            if (_searchType == SearchType.Unkown)
            {
                _searchType = SearchHelper.ParseSearchTypeFromSearchString(SearchString);
            }

            return _searchType;
        }
    }

    public static Search? Get(DataStore datastore, string name, string searchString)
    {
        var sql = "SELECT * FROM Search WHERE Name = @Name AND SearchString = @SearchString";
        var param = new { Name = name, SearchString = searchString };

        return datastore.Connection!.QueryFirstOrDefault<Search>(sql, param, null);
    }

    public static Search Add(DataStore datastore, string name, string searchString)
    {
        var search = new Search
        {
            Name = name,
            SearchString = searchString,
        };
        datastore.Connection.Insert<Search>(search);
        return search;
    }

    public static void Remove(DataStore datastore, string name, string searchString)
    {
        var sql = "DELETE FROM Search WHERE Name = @Name AND SearchString = @SearchString";
        var command = datastore.Connection!.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@Name", name);
        command.Parameters.AddWithValue("@SearchString", searchString);
        _log.Verbose(DataStore.GetCommandLogMessage(sql, command));
        var deleted = command.ExecuteNonQuery();
        _log.Verbose(DataStore.GetDeletedLogMessage(deleted));
    }

    public static IEnumerable<Search> GetAll(DataStore datastore)
    {
        return datastore.Connection.GetAll<Search>() ?? Enumerable.Empty<Search>();
    }

    public static void AddOrUpdate(DataStore datastore, string name, string searchString, bool isTopLevel)
    {
        var search = Get(datastore, name, searchString);

        search ??= Add(datastore, name, searchString);

        search.IsTopLevel = isTopLevel;
        datastore.Connection.Update<Search>(search);
    }

    public static IEnumerable<Search> GetAllTopLevel(DataStore datastore)
    {
        return datastore.Connection.Query<Search>("SELECT * FROM Search WHERE IsTopLevel");
    }
}
