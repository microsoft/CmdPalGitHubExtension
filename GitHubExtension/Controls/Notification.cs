// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.Controls;

public sealed class Notification : INotification
{
    public string Id { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string Reason { get; init; } = string.Empty;

    public string RepositoryFullName { get; init; } = string.Empty;

    public string HtmlUrl { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public bool Unread { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}
