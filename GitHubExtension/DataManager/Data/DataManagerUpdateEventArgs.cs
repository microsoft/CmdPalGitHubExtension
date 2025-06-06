// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.DataManager.Enums;

namespace GitHubExtension.DataManager.Data;

public delegate void DataManagerUpdateEventHandler(object? source, DataManagerUpdateEventArgs e);

public enum DataManagerUpdateKind
{
    Cancel,         // Update was cancelled.
    Error,          // An error occurred during update.
    Success,        // Update was successful.
}

public class DataManagerUpdateEventArgs : EventArgs
{
    private readonly DataManagerUpdateKind _kind;
    private readonly UpdateType _updateType;
    private readonly ISearch? _search;
    private readonly Exception? _exception;

    public DataManagerUpdateEventArgs(DataManagerUpdateKind updateKind, UpdateType updateType, ISearch? search = null, Exception? exception = null)
    {
        _kind = updateKind;
        _updateType = updateType;
        _search = search;
        _exception = exception;
    }

    public DataManagerUpdateKind Kind => _kind;

    public ISearch? Search => _search;

    public UpdateType UpdateType => _updateType;

    public Exception? Exception => _exception;
}
