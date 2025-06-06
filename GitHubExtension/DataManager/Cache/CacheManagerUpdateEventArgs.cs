// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;

namespace GitHubExtension.DataManager.Cache;

public delegate void CacheManagerUpdateEventHandler(object? source, CacheManagerUpdateEventArgs e);

public enum CacheManagerUpdateKind
{
    Started,
    Updated,
    Cleared,
    Error,
    Cancel,
    Account,
}

public class CacheManagerUpdateEventArgs : EventArgs
{
    private readonly CacheManagerUpdateKind _kind;
    private readonly Exception? _exception;
    private readonly ISearch? _search;

    public CacheManagerUpdateEventArgs(CacheManagerUpdateKind updateKind, ISearch? search = null, Exception? exception = null)
    {
        _kind = updateKind;
        _exception = exception;
        _search = search;
    }

    public CacheManagerUpdateKind Kind => _kind;

    public ISearch? Search => _search;

    public Exception? Exception => _exception;
}
