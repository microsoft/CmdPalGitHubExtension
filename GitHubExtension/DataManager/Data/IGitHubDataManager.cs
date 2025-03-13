// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.DataModel;
using GitHubExtension.DataModel.DataObjects;
using GitHubExtension.DataModel.Enums;

namespace GitHubExtension.DataManager.Data;

public interface IGitHubDataManager : IDisposable
{
    DataStoreOptions DataStoreOptions { get; }

    DateTime LastUpdated { get; set; }

    event DataManagerUpdateEventHandler? OnUpdate;

    Task RequestAllUpdateAsync(List<ISearch> searches, RequestOptions options);

    Task RequestSearchUpdateAsync(string name, string searchString, SearchType type, RequestOptions options);

    Search? GetSearch(string name, string searchString);
}
