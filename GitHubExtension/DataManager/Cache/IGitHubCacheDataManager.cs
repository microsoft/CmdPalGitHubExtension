// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.DataManager.Data;
using GitHubExtension.DataModel.Enums;

namespace GitHubExtension.DataManager.Cache;

public interface IGitHubCacheDataManager
{
    DateTime LastUpdated { get; set; }

    event DataManagerUpdateEventHandler? OnUpdate;

    Task RequestAllUpdateAsync(List<ISearch> searches, RequestOptions options);

    Task RequestSearchUpdateAsync(ISearch search, RequestOptions options);

    bool IsSearchNewOrStale(ISearch search, TimeSpan refreshCooldown);
}
