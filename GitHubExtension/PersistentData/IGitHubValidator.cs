// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;

namespace GitHubExtension.PersistentData;

public interface IGitHubValidator
{
    Task ValidateSearch(ISearch search);
}
