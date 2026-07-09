// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.Controls;

public interface IDiscussion
{
    string Title { get; }

    long Number { get; }

    string HtmlUrl { get; }

    string RepositoryFullName { get; }

    string Author { get; }

    DateTimeOffset UpdatedAt { get; }
}
