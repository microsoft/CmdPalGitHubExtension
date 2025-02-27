// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.DataManager;

public delegate void DataManagerUpdateEventHandler(object? source, DataManagerUpdateEventArgs e);

public enum DataManagerUpdateKind
{
    Cancel,         // Update was cancelled.
    Error,          // An error occurred during update.
    Success,        // Update was successful.
}

public class DataManagerUpdateEventArgs : EventArgs
{
    private readonly string _description;
    private readonly string[] _context;
    private readonly DataManagerUpdateKind _kind;
    private readonly UpdateType _updateType;
    private readonly Exception? _exception;

    public DataManagerUpdateEventArgs(DataManagerUpdateKind updateKind, UpdateType updateType, string updateDescription, string[] updateContext, Exception? exception = null)
    {
        _kind = updateKind;
        _description = updateDescription;
        _context = updateContext;
        _updateType = updateType;
        _exception = exception;
    }

    public DataManagerUpdateKind Kind => _kind;

    public string Description => _description;

    public string[] Context => _context;

    public UpdateType UpdateType => _updateType;

    public Exception? Exception => _exception;
}
