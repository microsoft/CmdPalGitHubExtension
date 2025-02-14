// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Client;
using GitHubExtension.DataModel.DataObjects;
using Octokit;

namespace GitHubExtension.Helpers;

public class SearchHelper
{
    private static readonly Lazy<SearchHelper> _instance = new(() => new SearchHelper(GitHubClientProvider.Instance.GetClient()));

    private readonly List<Search> _savedSearches;

    private GitHubClient _client;

    private SearchHelper(GitHubClient client)
    {
        _client = client;
        _savedSearches = new List<Search>();
    }

    public static SearchHelper Instance => _instance.Value;

    public void UpdateClient(GitHubClient client)
    {
        _client = client;
    }

    public List<Search> GetSavedSearches()
    {
        return _savedSearches;
    }

    public void AddSavedSearch(Search search)
    {
        _savedSearches.Add(search);
    }

    public void RemoveSavedSearch(Search search)
    {
        _savedSearches.Remove(search);
    }

    public void ClearSavedSearches()
    {
        _savedSearches.Clear();
    }

    // Runs the query saved and raises any Octokit errors
    public async Task ValidateSearch(Search search)
    {
        await _client.Search.SearchIssues(new SearchIssuesRequest(search?.SearchString));
    }
}
