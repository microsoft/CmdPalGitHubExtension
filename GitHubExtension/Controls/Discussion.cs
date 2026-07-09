// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.Controls;

public sealed class Discussion : IDiscussion
{
    public string Title { get; set; } = string.Empty;

    public long Number { get; set; }

    public string HtmlUrl { get; set; } = string.Empty;

    public string RepositoryFullName { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public DateTimeOffset UpdatedAt { get; set; }
}
