// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Helpers;

namespace GitHubExtension.DataManager.Data;

// ICloneSettingsStore backed by LocalSettings. The default clone location is
// %USERPROFILE%\source\repos, matching the common Visual Studio convention and
// staying inside a directory the user can normally write to.
public sealed class CloneSettingsStore : ICloneSettingsStore
{
    private const string ClonePathSettingKey = "ClonePath";

    public string GetDefaultCloneBaseDirectory()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "source", "repos");
    }

    public async Task<string> GetCloneBaseDirectoryAsync()
    {
        var configured = await LocalSettings.ReadSettingAsync<string>(ClonePathSettingKey);
        return string.IsNullOrWhiteSpace(configured) ? GetDefaultCloneBaseDirectory() : configured!;
    }

    public async Task SetCloneBaseDirectoryAsync(string path)
    {
        await LocalSettings.SaveSettingAsync(ClonePathSettingKey, path);
    }
}
