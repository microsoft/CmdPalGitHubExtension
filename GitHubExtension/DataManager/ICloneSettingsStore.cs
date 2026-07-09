// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.DataManager;

// Persists the configurable base directory into which repositories are cloned.
public interface ICloneSettingsStore
{
    // Returns the platform default clone base directory (used when nothing is
    // configured and as the placeholder in the clone form).
    string GetDefaultCloneBaseDirectory();

    // Returns the configured clone base directory, or the default when unset.
    Task<string> GetCloneBaseDirectoryAsync();

    // Persists the clone base directory preference.
    Task SetCloneBaseDirectoryAsync(string path);
}
