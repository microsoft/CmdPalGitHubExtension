// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Client;
using GitHubExtension.PersistentData;
using Octokit;

namespace GitHubExtension.Helpers;

public class SearchHelper
{
    private static readonly Lazy<SearchHelper> _instance = new(() => new SearchHelper(GitHubClientProvider.Instance.GetClient()));

    private GitHubClient _client;

    private SearchHelper(GitHubClient client)
    {
        _client = client;
    }

    public static SearchHelper Instance => _instance.Value;

    public void UpdateClient(GitHubClient client)
    {
        _client = client;
    }

    public async Task<IEnumerable<PersistentData.Search>> GetSavedSearches()
    {
        var dataManager = PersistentDataManager.CreateInstance();
        return await dataManager!.GetAllSearchesAsync();
    }

    public async void AddSavedSearch(SearchCandidate search)
    {
        // FIXME: Either add SearchCandidate to GitHubDataManager as well or move the Get
        // Search function to GitHubDataManager
        var dataManager = PersistentDataManager.CreateInstance();
        await dataManager!.AddSearchAsync(search.Name, search.SearchString, search.Type);
    }

    public async void RemoveSavedSearch(SearchCandidate search)
    {
        // FIXME: Calling this didn't remove search
        var dataManager = PersistentDataManager.CreateInstance();
        await dataManager!.RemoveSearchAsync(search.Name, search.SearchString, search.Type);
    }

    // TODO: update and/or delete this method
    public async void RemoveSavedSearch(Search search)
    {
        var dataManager = PersistentDataManager.CreateInstance();
        await dataManager!.RemoveSearchAsync(search.Name, search.SearchString, DataModel.Enums.SearchType.Issues);
    }

    public void ClearSavedSearches()
    {
        // TODO: Implement
    }

    // Runs the query saved and raises any Octokit errors
    public async Task ValidateSearch(SearchCandidate search)
    {
        await _client.Search.SearchIssues(new SearchIssuesRequest(search?.SearchString));
    }
}
