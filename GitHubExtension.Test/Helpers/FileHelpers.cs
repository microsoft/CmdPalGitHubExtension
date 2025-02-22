// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.Test;

public partial class TestHelpers
{
    public static string CreateUniqueFolderName(string prefix)
    {
        // This could potentially be too long of a path name,
        // but should be OK for now. Keep the prefix short.
        return $"{prefix}-{Guid.NewGuid()}";
    }

    public static string GetUniqueFolderPath(string prefix)
    {
        return Path.Combine(Path.GetTempPath(), CreateUniqueFolderName(prefix));
    }
}
