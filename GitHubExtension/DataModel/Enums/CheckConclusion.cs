// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.DataModel;

public enum CheckConclusion
{
    // Ordered by degree of success.
    Unknown = -1,
    None = 0,
    Failure = 1,
    TimedOut = 2,
    Cancelled = 3,
    ActionRequired = 4,
    Stale = 5,
    Neutral = 6,
    Success = 7,
    Skipped = 8,
}
