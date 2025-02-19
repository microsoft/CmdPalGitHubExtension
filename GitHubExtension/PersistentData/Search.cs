﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Dapper;
using Dapper.Contrib.Extensions;
using GitHubExtension.DataModel;
using GitHubExtension.DataModel.Enums;
using GitHubExtension.Helpers;

namespace GitHubExtension.PersistentData;

[Table("Search")]
public class Search
{
    [Key]
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string SearchString { get; set; } = string.Empty;

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
        datastore.Connection.Delete(new Search { Name = name, SearchString = searchString });
    }

    public static IEnumerable<Search> GetAll(DataStore datastore)
    {
        return datastore.Connection.GetAll<Search>() ?? Enumerable.Empty<Search>();
    }
}
