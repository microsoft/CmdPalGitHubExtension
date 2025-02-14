// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.DataModel;

public enum CheckStatus
{
    // Ordered by state from not started to completed.
    Unknown = -1,
    None = 0,
    Queued = 1,
    InProgress = 2,
    Completed = 3,
}
