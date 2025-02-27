// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.Pages;

public interface IPullRequest
{
    string Title { get; }

    long Number { get; }

    string SourceBranch { get; }

    string Body { get; }

    string HtmlUrl { get; }

    IEnumerable<ILabel> Labels { get; }
}
