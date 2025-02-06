// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Forms;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Pages;

internal sealed partial class SaveQueryPage : FormPage
{
    private readonly SaveQueryForm _saveQueryForm;

#pragma warning disable IDE0044 // Add readonly modifier
    private StatusMessage _saveQueryStatusMessage;
#pragma warning restore IDE0044 // Add readonly modifier

    public SaveQueryPage()
    {
        _saveQueryForm = new();
        _saveQueryForm.QuerySaved += OnQuerySaved;
        _saveQueryForm.LoadingStateChanged += OnLoadingChanged;
        _saveQueryStatusMessage = new StatusMessage();
    }

    public override IForm[] Forms()
    {
        ExtensionHost.HideStatus(_saveQueryStatusMessage);
        return new IForm[] { _saveQueryForm };
    }

    private void OnQuerySaved(object sender, object? args)
    {
        IsLoading = false;
        if (args is Exception ex)
        {
            _saveQueryStatusMessage.Message = $"Error in saving query: {ex.Message}";
            _saveQueryStatusMessage.State = MessageState.Error;
        }
        else
        {
            _saveQueryStatusMessage.Message = "Query saved successfully!";
            _saveQueryStatusMessage.State = MessageState.Success;
        }

        var toast = new ToastStatusMessage(_saveQueryStatusMessage);
        toast.Show();
    }

    private void OnLoadingChanged(object sender, bool isLoading)
    {
        IsLoading = isLoading;
    }
}
