// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.Controls;

public interface INotification
{
    string Id { get; }

    string Title { get; }

    string Reason { get; }

    string RepositoryFullName { get; }

    string HtmlUrl { get; }

    string Type { get; }

    bool Unread { get; }

    DateTimeOffset UpdatedAt { get; }
}
