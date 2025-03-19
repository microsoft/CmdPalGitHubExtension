// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.Helpers;

public class ExtensionConstants
{
    public static readonly TimeSpan UpdateInterval = TimeSpan.FromMinutes(10);

    public static readonly TimeSpan RefreshCooldown = TimeSpan.FromMinutes(2);

    public static readonly int PerPage = 100;
}
