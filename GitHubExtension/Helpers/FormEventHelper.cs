// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls.Forms;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Helpers;

public static class FormEventHelper
{
    public static void WireFormEvents(
        IGitHubForm form,
        ContentPage page,
        StatusMessage statusMessage,
        string successMessage,
        string errorMessage)
    {
        form.FormSubmitted += (sender, args) => OnFormSubmit(page, statusMessage, successMessage, errorMessage, args);
        form.LoadingStateChanged += (sender, isLoading) => OnLoadingStateChanged(page, isLoading);
    }

    public static void OnFormSubmit(
        ContentPage page,
        StatusMessage statusMessage,
        string successMessage,
        string errorMessage,
        FormSubmitEventArgs? args)
    {
        if (args?.Exception != null)
        {
            var message = $"{errorMessage}: {args.Exception.Message}";
            ExtensionHost.LogMessage(new LogMessage() { Message = args.Exception.Message });
            SetStatusMessage(statusMessage, message, MessageState.Error);
            ExtensionHost.ShowStatus(statusMessage, StatusContext.Page);
        }
        else
        {
            SetStatusMessage(statusMessage, successMessage, MessageState.Success);
            ToastStatusMessage(statusMessage);
        }
    }

    public static void OnLoadingStateChanged(ContentPage page, bool isLoading)
    {
        page.IsLoading = isLoading;
    }

    public static void SetStatusMessage(StatusMessage statusMessage, string message, MessageState state)
    {
        statusMessage.Message = message;
        statusMessage.State = state;
    }

    public static void ToastStatusMessage(StatusMessage statusMessage)
    {
        var toast = new ToastStatusMessage(statusMessage);
        toast.Show();
    }
}
