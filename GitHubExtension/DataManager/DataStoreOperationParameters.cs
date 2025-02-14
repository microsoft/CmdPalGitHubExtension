// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataManager;
using GitHubExtension.DataModel.Enums;
using GitHubExtension.DeveloperId;

namespace GitHubExtension;

public class DataStoreOperationParameters
{
    // parameters for updating the data store.
    public string? Owner { get; set; }

    public string? RepositoryName { get; set; }

    public string OperationName { get; set; } = string.Empty;

    public UpdateType UpdateType { get; set; } = UpdateType.Unknown;

    public string? SearchName { get; set; }

    public SearchType SearchType { get; set; } = SearchType.Unkown;

    public IEnumerable<IDeveloperId> DeveloperIds { get; set; } = Enumerable.Empty<IDeveloperId>();

    public RequestOptions? RequestOptions { get; set; }

    public DataStoreOperationParameters()
    {
    }

    public override string ToString()
    {
        return $"{OperationName} - {RequestOptions}";
    }
}
