// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.DataManager;

// Coordinates cloning a GitHub repository to the local machine: resolves the
// clone URL and destination directory, validates git availability, and delegates
// the actual clone to IGitService.
public interface IRepositoryCloneManager
{
    // Clones the given repository (an owner/repo string, a full GitHub URL, or a
    // clone URL) into targetDirectory, or the configured base directory when
    // targetDirectory is null/empty. Returns the local path of the clone.
    Task<string> CloneRepositoryAsync(string repository, string? targetDirectory = null, CancellationToken cancellationToken = default);
}
