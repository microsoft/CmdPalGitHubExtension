// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls.Commands;
using GitHubExtension.Controls.ListItems;
using GitHubExtension.Controls.Pages;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls;

public class savedSearchesMediator
{
    public event EventHandler<object?>? SearchRemoving;

    public event EventHandler<object?>? SearchRemoved;

    public event EventHandler<object?>? SearchSaved;

    public savedSearchesMediator()
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
