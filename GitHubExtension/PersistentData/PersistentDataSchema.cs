// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataModel;

namespace GitHubExtension.PersistentData;

public sealed class PersistentDataSchema : IDataStoreSchema
{
    public long SchemaVersion => 1;

    public List<string> SchemaSqls => _schemaSqlsValue;

    private const string Repository =
        @"CREATE TABLE IF NOT EXISTS Repository (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            OwnerLogin TEXT NOT NULL
        )";

    private const string Search =
        @"CREATE TABLE IF NOT EXISTS Search (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            SearchString TEXT NOT NULL
        )";

    private static readonly List<string> _schemaSqlsValue = new()
    {
        Repository,
        Search,
    };
}
