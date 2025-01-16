// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;

namespace GitHubExtension.Helpers;

public static class StringExtensions
{
    public static string ToStringInvariant<T>(this T value) => Convert.ToString(value, CultureInfo.InvariantCulture)!;

    public static string FormatInvariant(this string value, params object[] arguments) => string.Format(CultureInfo.InvariantCulture, value, arguments);
}
