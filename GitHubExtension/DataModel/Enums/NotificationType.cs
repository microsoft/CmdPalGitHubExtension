// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.DataModel;

public enum NotificationType
{
    Unknown = 0,
    CheckRunFailed = 1,
    CheckRunSucceeded = 2,
    NewReview = 3,
}
