// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Helpers;

public static class ToastHelper
{
    public static void ShowToast(string message, MessageState state)
    {
        var statusMessage = new StatusMessage
        {
            Message = message,
            State = state,
        };

        var toast = new ToastStatusMessage(statusMessage);
        toast.Show();
    }

    public static void ShowSuccessToast(string message)
    {
        ShowToast(message, MessageState.Success);
    }

    public static void ShowErrorToast(string message)
    {
        ShowToast(message, MessageState.Error);
    }
}
