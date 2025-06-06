// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.DataManager.Enums;

namespace GitHubExtension.DataManager.Data;

public class DataUpdateParameters
{
    public string OperationName { get; set; } = string.Empty;

    public UpdateType UpdateType { get; set; } = UpdateType.Unknown;

    public ISearch? Search { get; set; }

    public RequestOptions? RequestOptions { get; set; }

    public DataUpdateParameters()
    {
    }

    public override string ToString()
    {
        return $"{OperationName} - {RequestOptions}";
    }
}
