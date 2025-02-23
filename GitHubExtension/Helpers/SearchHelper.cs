﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Client;
using GitHubExtension.DataModel.Enums;
using GitHubExtension.PersistentData;
using Octokit;

namespace GitHubExtension.Helpers;

public class SearchHelper : ISearchHelper
{
    private static readonly Lazy<SearchHelper> _instance = new(() => new SearchHelper(GitHubClientProvider.Instance.GetClient()));

    private readonly PersistentDataManager _dataManager;

    private GitHubClient _client;

    public SearchHelper(GitHubClient client)
    {
        _client = client;
        _dataManager = PersistentDataManager.CreateInstance()!;
    }

    public static SearchHelper Instance => _instance.Value;

    public void UpdateClient(GitHubClient client)
    {
        _client = client;
    }

    public async Task<IEnumerable<PersistentData.Search>> GetSavedSearches()
    {
        return await _dataManager.GetAllSearchesAsync();
    }

    public async Task AddSavedSearch(SearchCandidate search)
    {
        await _dataManager.AddSearchAsync(search.Name, search.SearchString, search.Type);
    }

    public async Task RemoveSavedSearch(SearchCandidate search)
    {
        await _dataManager.RemoveSearchAsync(search.Name, search.SearchString, search.Type);
    }

    public async Task RemoveSavedSearch(Search search)
    {
        await _dataManager.RemoveSearchAsync(search.Name, search.SearchString, DataModel.Enums.SearchType.Issues);
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

    public static SearchType ParseSearchTypeFromSearchString(string searchString)
    {
        // parse "type:typeName" if it's in the string
        var type = searchString.Split(' ').FirstOrDefault(x => x.StartsWith("type:", StringComparison.OrdinalIgnoreCase));
        if (type != null)
        {
            var typeName = type.Split(':')[1];
            if (SearchTypeMappings.TryGetValue(typeName.ToLower(CultureInfo.CurrentCulture), out var searchType))
            {
                return searchType;
            }

            return (SearchType)Enum.Parse(typeof(SearchType), typeName, true);
        }

        // parse "is:typeName" if it's in the string
        type = searchString.Split(' ').FirstOrDefault(x => x.StartsWith("is:", StringComparison.OrdinalIgnoreCase));
        if (type != null)
        {
            var typeName = type.Split(':')[1];
            if (SearchTypeMappings.TryGetValue(typeName.ToLower(CultureInfo.CurrentCulture), out var searchType))
            {
                return searchType;
            }

            return (SearchType)Enum.Parse(typeof(SearchType), typeName, true);
        }

        return SearchType.Unkown;
    }

    private static readonly Dictionary<string, SearchType> SearchTypeMappings = new()
    {
        { "issue", SearchType.Issues },
        { "issues", SearchType.Issues },
        { "pr", SearchType.PullRequests },
        { "pullrequest", SearchType.PullRequests },
        { "repository", SearchType.Repositories },
        { "repo", SearchType.Repositories },
    };
}
