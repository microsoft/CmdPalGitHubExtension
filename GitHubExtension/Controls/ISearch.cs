// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataModel.Enums;

namespace GitHubExtension.Pages;

public interface ISearch
{
    string Name { get; }

    string SearchString { get; }

    SearchType Type { get; }
}
