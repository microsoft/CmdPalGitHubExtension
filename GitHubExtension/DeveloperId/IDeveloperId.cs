// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.DeveloperIds;

public interface IDeveloperId
{
    string LoginId { get; }

    string Url { get; }

    Octokit.IGitHubClient GitHubClient { get; }
}
