// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.DataManager.Enums;

namespace GitHubExtension.DataManager.Cache;

public interface ICacheManager
{
    event CacheManagerUpdateEventHandler? OnUpdate;

    Task RequestRefresh(ISearch search);
}
