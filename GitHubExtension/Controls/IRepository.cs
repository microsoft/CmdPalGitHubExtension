// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.Controls;

public interface IRepository
{
    string Name { get; }

    string FullName { get; }

    string Description { get; }

    string HtmlUrl { get; }

    string CloneUrl { get; }

    string OwnerLogin { get; }

    bool IsPrivate { get; }

    bool IsFork { get; }

    string DefaultBranch { get; }
}
