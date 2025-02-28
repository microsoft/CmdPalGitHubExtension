// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Forms.Templates;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.PageTemplates;

public abstract partial class GitHubContentPage : ContentPage, IGitHubPage
{
    public abstract StatusMessage StatusMessage { get; set; }

    public abstract string SuccessMessage { get; set; }

    public abstract string ErrorMessage { get; set; }

    public abstract GitHubForm PageForm { get; set; }

    public GitHubContentPage()
    {
        ExtensionHost.HideStatus(StatusMessage);
    }

    public override IContent[] GetContent()
    {
        ExtensionHost.HideStatus(StatusMessage);
        return [PageForm];
    }

    public virtual void OnFormSubmit(object sender, FormSubmitEventArgs? args)
    {
        if (args?.Exception != null)
        {
            ErrorMessage = $"{ErrorMessage}: {args.Exception.Message}";
            ExtensionHost.LogMessage(new LogMessage() { Message = $"{args.Exception.Message}" });
            SetStatusMessage(ErrorMessage, MessageState.Error);
            ExtensionHost.ShowStatus(StatusMessage);
        }
        else
        {
            SetStatusMessage(SuccessMessage, MessageState.Success);
            ToastStatusMessage();
        }

        return;
    }

    public void OnLoadingStateChanged(object sender, bool isLoading)
    {
        IsLoading = isLoading;
    }

    public void SetStatusMessage(string message, MessageState state)
    {
        StatusMessage.Message = message;
        StatusMessage.State = state;
    }

    public void ToastStatusMessage()
    {
        var toast = new ToastStatusMessage(StatusMessage);
        toast.Show();
    }
}
