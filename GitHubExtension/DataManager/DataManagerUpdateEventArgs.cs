// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.DataManager;

public delegate void DataManagerUpdateEventHandler(object? source, DataManagerUpdateEventArgs e);

public enum DataManagerUpdateKind
{
    Repository,     // Single repository was updated.
    Developer,      // Developer content was updated, a thin slice of the data across multiple repositories.
    Query,          // A custom query was updated, which could be any amount of data in the datastore.
    PullRequests,   // All Pull Requests updated.
    Issues,         // All issues updated.
    Searches,       // Searches updated.
    Search,         // A single search was updated.
    All,            // All data: PRs, Issues and Searches
    Cancel,         // Update was cancelled.
    Error,         // An error occurred during update.
}

public class DataManagerUpdateEventArgs : EventArgs
{
    private readonly string _description;
    private readonly string[] _context;
    private readonly DataManagerUpdateKind _kind;

    public DataManagerUpdateEventArgs(DataManagerUpdateKind updateKind, string updateDescription, string[] updateContext)
    {
        _kind = updateKind;
        _description = updateDescription;
        _context = updateContext;
    }

    public DataManagerUpdateKind Kind => _kind;

    public string Description => _description;

    public string[] Context => _context;
}
