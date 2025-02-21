// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Pages;

internal abstract partial class GitHubFormPage : FormPage, IGitHubPage
{
    public abstract StatusMessage StatusMessage { get; }

    public abstract string SuccessMessage { get; }

    public abstract string ErrorMessage { get; }

    public virtual GitHubForm? PageForm { get; set; }

    public GitHubFormPage()
    {
        if (StatusMessage != null)
        {
            ExtensionHost.HideStatus(StatusMessage);
        }
    }

    public override IForm[] Forms()
    {
        ExtensionHost.HideStatus(StatusMessage);
        return new IForm[] { PageForm ?? throw new InvalidOperationException() };
    }

    public virtual void OnFormSubmit(object sender, FormSubmitEventArgs? args)
    {
        // LoadingStateChanged will stop loading
        if (args?.Exception != null)
        {
            ExtensionHost.LogMessage(new LogMessage() { Message = ErrorMessage });
            SetStatusMessage(StatusMessage, ErrorMessage, MessageState.Error);
            ExtensionHost.ShowStatus(StatusMessage);
        }
        else
        {
            SetStatusMessage(StatusMessage, SuccessMessage, MessageState.Success);
            ToastStatusMessage(StatusMessage);
        }

        return;
    }

    public void OnLoadingStateChanged(object sender, bool isLoading)
    {
        IsLoading = isLoading;
    }

    public void SetStatusMessage(StatusMessage statusMessage, string message, MessageState state)
    {
        statusMessage.Message = message;
        statusMessage.State = state;
    }

    public void ToastStatusMessage(StatusMessage statusMessage)
    {
        var toast = new ToastStatusMessage(statusMessage);
        toast.Show();
    }
}
