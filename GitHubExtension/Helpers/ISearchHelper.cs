// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Octokit;

namespace GitHubExtension.Helpers;

public interface ISearchHelper
{
    public void UpdateClient(GitHubClient client);

    public Task<IEnumerable<PersistentData.Search>> GetSavedSearches();

    public Task AddSavedSearch(SearchCandidate search);

    public Task RemoveSavedSearch(SearchCandidate search);

    public Task RemoveSavedSearch(PersistentData.Search search);

    public Task ValidateSearch(SearchCandidate search);
}
