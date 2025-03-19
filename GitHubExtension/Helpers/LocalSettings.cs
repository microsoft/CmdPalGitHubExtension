// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.Storage;

namespace GitHubExtension.Helpers;

public static class LocalSettings
{
    private static readonly string _applicationDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CommandPalette/ApplicationData");
    private static readonly string _localSettingsFile = "LocalSettings.json";

    private static Dictionary<string, object>? _settings;

    private static async Task InitializeAsync()
    {
        if (_settings == null)
        {
            if (RuntimeHelper.IsMSIX)
            {
                _settings = new Dictionary<string, object>();
            }
            else
            {
                _settings = await Task.Run(() => FileHelper.Read<Dictionary<string, object>>(_applicationDataFolder, _localSettingsFile)) ?? new Dictionary<string, object>();
            }
        }
    }

    public static async Task<T?> ReadSettingAsync<T>(string key)
    {
        await InitializeAsync();

        if (_settings != null)
        {
            if (_settings.TryGetValue(key, out var obj))
            {
                return await Json.ToObjectAsync<T>((string)obj);
            }
            else
            {
                if (RuntimeHelper.IsMSIX)
                {
                    if (ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out var obj2))
                    {
                        _settings![key] = obj2;
                        return await Json.ToObjectAsync<T>((string)obj2);
                    }
                }
            }
        }

        return default;
    }

    public static async Task SaveSettingAsync<T>(string key, T value)
    {
        await InitializeAsync();

        if (_settings != null)
        {
            _settings![key] = await Json.StringifyAsync(value!);

            if (RuntimeHelper.IsMSIX)
            {
                ApplicationData.Current.LocalSettings.Values[key] = _settings![key];
            }
            else
            {
                await Task.Run(() => FileHelper.Save(_applicationDataFolder, _localSettingsFile, _settings));
            }
        }
    }
}
