// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.Controls;

public class SavedSearchesMediator
{
    public event EventHandler<object?>? SearchRemoving;

    public event EventHandler<object?>? SearchRemoved;

    public event EventHandler<object?>? SearchSaved;

    public SavedSearchesMediator()
    {
    }

    public void RemovingSearch(object args)
    {
        SearchRemoving?.Invoke(this, args);
    }

    public void RemoveSearch(object args)
    {
        SearchRemoved?.Invoke(this, args);
    }

    public void AddSearch(object args)
    {
        SearchSaved?.Invoke(this, args);
    }
}
