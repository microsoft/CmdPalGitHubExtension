// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Helpers;

namespace GitHubExtension.Controls;

public class SavedSearchesMediator
{
    public event EventHandler<object?>? SearchRemoving;

    public event EventHandler<SavedSearchesUpdatedEventArgs>? SearchRemoved;

    public event EventHandler<SavedSearchesUpdatedEventArgs>? SearchSaved;

    public SavedSearchesMediator()
    {
    }

    public void RemovingSearch(object args)
    {
        SearchRemoving?.Invoke(this, args);
    }

    public void RemoveSearch(SavedSearchesUpdatedEventArgs args)
    {
        SearchRemoved?.Invoke(this, args);
    }

    public void AddSearch(SavedSearchesUpdatedEventArgs args)
    {
        SearchSaved?.Invoke(this, args);
    }
}
