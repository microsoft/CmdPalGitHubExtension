// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.DataManager;

// Abstraction over the local git executable so the clone flow can be unit
// tested without spawning a real process or requiring git on the machine.
public interface IGitService
{
    // Returns true when a git executable can be located on the machine.
    bool IsGitInstalled();

    // Clones the repository at the given URL into the destination directory.
    // Throws when git is unavailable or the clone fails.
    Task CloneAsync(string cloneUrl, string destinationPath, CancellationToken cancellationToken = default);
}
