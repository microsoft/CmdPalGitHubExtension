// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GitHubExtension.Helpers;

public static class Json
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        IncludeFields = true,
    };

    public static async Task<T> ToObjectAsync<T>(string value)
    {
        return typeof(T) == typeof(bool)
            ? (T)(object)bool.Parse(value)
            : await Task.Run<T>(() =>
        {
            return JsonConvert.DeserializeObject<T>(value)!;
        });
    }

    public static async Task<string> StringifyAsync<T>(T value)
    {
        return typeof(T) == typeof(bool)
            ? value!.ToString()!.ToLowerInvariant()
            : await Task.Run<string>(() =>
        {
            return JsonConvert.SerializeObject(value);
        });
    }

    public static string Stringify<T>(T value, JsonSerializerOptions? options = null)
    {
        return typeof(T) == typeof(bool)
            ? value!.ToString()!.ToLowerInvariant()
            : System.Text.Json.JsonSerializer.Serialize(value, options ?? _options);
    }

    public static T? ToObject<T>(string json) => typeof(T) == typeof(bool) ? (T)(object)bool.Parse(json) : System.Text.Json.JsonSerializer.Deserialize<T>(json, _options);
}
