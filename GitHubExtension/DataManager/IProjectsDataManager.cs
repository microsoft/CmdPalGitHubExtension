// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;

namespace GitHubExtension.DataManager;

public interface IProjectsDataManager
{
    // Returns the signed-in user's Projects (v2), via the GraphQL viewer field.
    Task<IEnumerable<IProject>> GetMyProjectsAsync();
}
