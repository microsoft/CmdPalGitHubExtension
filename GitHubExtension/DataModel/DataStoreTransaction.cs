// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Data.Sqlite;

namespace GitHubExtension.DataModel;

public class DataStoreTransaction : IDataStoreTransaction
{
    private SqliteTransaction? _transaction;

    public static IDataStoreTransaction BeginTransaction(DataStore dataStore)
    {
        if (dataStore != null)
        {
            if (dataStore.Connection != null)
            {
                return new DataStoreTransaction(dataStore.Connection.BeginTransaction());
            }
        }

        return new DataStoreTransaction(null);
    }

    private DataStoreTransaction(SqliteTransaction? tx)
    {
        _transaction = tx;
    }

    public void Commit()
    {
        _transaction?.Commit();
    }

    public void Rollback()
    {
        _transaction?.Rollback();
    }

    private bool _disposed; // To detect redundant calls.

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transaction?.Dispose();
                _transaction = null;
            }

            _disposed = true;
        }
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
