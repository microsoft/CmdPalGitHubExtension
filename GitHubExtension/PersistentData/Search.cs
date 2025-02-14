// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Dapper;
using Dapper.Contrib.Extensions;
using GitHubExtension.DataModel;
using GitHubExtension.DataModel.Enums;

namespace GitHubExtension.PersistentData;

[Table("Search")]
public class Search
{
    [Key]
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string SearchString { get; set; } = string.Empty;

    public long TypeId { get; set; } = DataStore.NoForeignKey;

    public static Search? Get(DataStore datastore, string name, string searchString, SearchType type)
    {
        var sql = "SELECT * FROM Search WHERE Name = @Name AND SearchString = @SearchString AND TypeId = @Type";
        var param = new { Name = name, SearchString = searchString, TypeId = (long)type };

        return datastore.Connection!.QueryFirstOrDefault<Search>(sql, param, null);
    }

    public static Search Add(DataStore datastore, string name, string searchString, SearchType type)
    {
        var search = new Search
        {
            Name = name,
            SearchString = searchString,
            TypeId = (long)type,
        };
        datastore.Connection.Insert<Search>(search);
        return search;
    }

    public static void Remove(DataStore datastore, string name, string searchString, SearchType type)
    {
        datastore.Connection.Delete(new Search { Name = name, SearchString = searchString, TypeId = (long)type });
    }

    public static IEnumerable<Search> GetAll(DataStore datastore)
    {
        return datastore.Connection.GetAll<Search>() ?? Enumerable.Empty<Search>();
    }
}
