// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;

namespace GitHubExtension.Helpers;

public static class DateTimeExtensions
{
    // Data store stores time as integers, which is just the Ticks so we don't lose precision.
    // This extension provides converters for storing the values as either TEXT or INTEGER,
    // Although SQLite types are suggestions, it may be internally represented differently.
    public static long ToDataStoreInteger(this DateTime value)
    {
        return value.Ticks;
    }

    public static long ToDataStoreInteger(this TimeSpan value)
    {
        return value.Ticks;
    }

    public static DateTime ToDateTime(this long value)
    {
        return new DateTime(value, DateTimeKind.Utc);
    }

    public static TimeSpan ToTimeSpan(this long value)
    {
        return new TimeSpan(value);
    }

    public static string ToDataStoreString(this DateTime value)
    {
        return value.ToDataStoreInteger().ToStringInvariant();
    }

    public static string ToDataStoreString(this TimeSpan value)
    {
        return value.ToDataStoreInteger().ToStringInvariant();
    }

    public static DateTime ToDateTime(this string value)
    {
        return long.Parse(value, CultureInfo.InvariantCulture).ToDateTime();
    }

    public static TimeSpan ToTimeSpan(this string value)
    {
        return long.Parse(value, CultureInfo.InvariantCulture).ToTimeSpan();
    }
}
