// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;

namespace GitHubExtension;

public interface IPagesFactory
{
    public IPage GetSaveSearchPage();

    public IPage GetSaveSearchPage(SearchInput input);

    public IPage GetSavedSearchesPage();

    public IPage GetEditSearchPage(PersistentData.Search search);

    public IInvokableCommand GetRemoveSavedSearchCommand(SearchCandidate search);

    public IInvokableCommand GetRemoveSavedSearchCommand(PersistentData.Search search);

    public IPage CreateForSearch(PersistentData.Search search);
}
