// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.Win32;
using Windows.Win32.Foundation;

namespace GitHubExtension.Helpers;

public static class RuntimeHelper
{
    public static bool IsMSIX
    {
        get
        {
            // TODO: Fix this
            // uint length = 0;
            // return PInvoke.GetCurrentPackageFullName(ref length, null) != WIN32_ERROR.APPMODEL_ERROR_NO_PACKAGE;
            #pragma warning disable
            return false;
        }
    }
}
