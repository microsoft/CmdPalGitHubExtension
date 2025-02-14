// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.DataModel;

public enum PullRequestCombinedStatus
{
    Unknown = -1,

    /// <summary>
    /// There are no statuses.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Error in the build.
    /// </summary>
    Failed = 1,

    /// <summary>
    /// Reported failure.
    /// </summary>
    Success = 2,
}
