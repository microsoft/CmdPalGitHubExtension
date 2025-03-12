// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Nodes;
using GitHubExtension.Controls;
using GitHubExtension.Controls.Commands;
using GitHubExtension.Controls.Forms;
using GitHubExtension.Controls.Pages;
using GitHubExtension.DataModel;
using GitHubExtension.Helpers;
using GitHubExtension.PersistentData;
using Microsoft.CommandPalette.Extensions;
using Moq;

namespace GitHubExtension.Test.Controls;

public partial class TestSavedSearchesPage : SavedSearchesPage
{
    public event Action? OnSearchSavedCalled;

    public TestSavedSearchesPage(
        ISearchPageFactory searchPageFactory,
        ISearchRepository searchRepository,
        IListItem addSearchListItem)
        : base(searchPageFactory, searchRepository, addSearchListItem)
    {
    }

    public override void OnSearchSaved(object sender, object? args)
    {
        base.OnSearchSaved(sender, args);
        OnSearchSavedCalled?.Invoke();
    }
}
