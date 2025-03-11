// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.Controls;

public interface ISearchRepository
{
    ISearch GetSearch(string name, string searchString);

    Task<IEnumerable<ISearch>> GetSavedSearches();

    Task RemoveSavedSearch(ISearch search);

    Task ValidateSearch(ISearch search);

    Task AddSavedSearch(ISearch search);

    Task<IEnumerable<ISearch>> GetTopLevelSearches();

    Task<bool> IsTopLevel(ISearch search);

    Task UpdateSearchTopLevelStatus(ISearch search, bool isTopLevel);
}
