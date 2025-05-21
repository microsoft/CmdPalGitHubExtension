// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;

namespace GitHubExtension.Helpers;

public class SavedSearchRemovedEventArgs : EventArgs
{
    public bool Status { get; }

    public Exception? Exception { get; }

    public ISearch? Search { get; }

    public SavedSearchRemovedEventArgs(bool status, Exception? ex, ISearch search)
    {
        Status = status;
        Exception = ex;
        Search = search;
    }
}
