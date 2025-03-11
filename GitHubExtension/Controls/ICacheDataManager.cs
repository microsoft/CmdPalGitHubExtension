// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataManager.Cache;

namespace GitHubExtension.Controls;

public interface ICacheDataManager
{
    // Not sure about this yet. Exposing event from another layer.
    event CacheManagerUpdateEventHandler? OnUpdate;

    Task<IEnumerable<IIssue>> GetIssues(ISearch search);

    Task<IEnumerable<IPullRequest>> GetPullRequests(ISearch search);

    Task<IEnumerable<IIssue>> GetIssuesAndPullRequests(ISearch search);
}
