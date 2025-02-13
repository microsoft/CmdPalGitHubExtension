// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.DataManager;

// To be used with Cache Manager to indicate the type of update.
public enum RefreshKind
{
    PullRequests,
    Issues,
    Searches,
    All,
}
