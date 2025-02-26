﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataManager;

namespace GitHubExtension.Pages;

public interface ICacheDataManager
{
    Task Refresh(UpdateType updateType, ISearch search);

    // Not sure about this yet. Exposing event from another layer.
    public event CacheManagerUpdateEventHandler? OnUpdate;

    Task<IEnumerable<IIssue>> GetIssues(ISearch search);

    Task<IEnumerable<IPullRequest>> GetPullRequests(ISearch search);

    void CancelUpdateInProgress();
}
