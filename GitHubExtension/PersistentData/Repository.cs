// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Dapper;
using Dapper.Contrib.Extensions;
using GitHubExtension.DataModel;

namespace GitHubExtension.PersistentData;

[Table("Repository")]
public class Repository
{
    [Key]
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string OwnerLogin { get; set; } = string.Empty;

    [Write(false)]
    private DataStore? DataStore
    {
        get; set;
    }

    [Write(false)]
    [Computed]
    public string FullName => $"{OwnerLogin}/{Name}";

    public static Repository Add(DataStore datastore, string owner, string name)
    {
        var repository = new Repository
        {
            OwnerLogin = owner,
            Name = name,
        };
        datastore.Connection.Insert<Repository>(repository);
        return repository;
    }

    public static void Remove(DataStore datastore, string owner, string name)
    {
        datastore.Connection.Delete(new Repository { OwnerLogin = owner, Name = name });
    }

    public static Repository? Get(DataStore datastore, string owner, string name)
    {
        var sql = "SELECT * FROM Repository WHERE OwnerLogin = @OwnerLogin AND Name = @Name";
        var param = new { OwnerLogin = owner, Name = name };

        return datastore.Connection!.QueryFirstOrDefault<Repository>(sql, param, null);
    }

    public static IEnumerable<Repository> GetAll(DataStore datastore)
    {
        return datastore.Connection.GetAll<Repository>() ?? Enumerable.Empty<Repository>();
    }
}
