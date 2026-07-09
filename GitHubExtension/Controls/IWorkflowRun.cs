// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.Controls;

public interface IWorkflowRun
{
    long Id { get; }

    string Name { get; }

    string Status { get; }

    string Conclusion { get; }

    string HeadBranch { get; }

    string TriggerEvent { get; }

    long RunNumber { get; }

    string HtmlUrl { get; }

    string RepositoryFullName { get; }

    DateTimeOffset CreatedAt { get; }
}
