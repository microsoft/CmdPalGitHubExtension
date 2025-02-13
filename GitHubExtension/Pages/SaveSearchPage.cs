// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Forms;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Pages;

internal sealed partial class SaveSearchPage : FormPage
{
    private readonly StatusMessage _saveSearchStatusMessage;

    private readonly SearchInput _searchInput;

    public SaveSearchPage()
        : this(SearchInput.SearchString)
    {
    }

    public SaveSearchPage(SearchInput input)
    {
        _searchInput = input;
        SaveSearchForm.SearchSaved += OnSearchSaved;
        SaveSearchForm.LoadingStateChanged += OnLoadingChanged;
        _saveSearchStatusMessage = new StatusMessage();
        ExtensionHost.HideStatus(_saveSearchStatusMessage);
    }

    public override IForm[] Forms()
    {
        ExtensionHost.HideStatus(_saveSearchStatusMessage);
        return [new SaveSearchForm(_searchInput)];
    }

    private void OnSearchSaved(object sender, object? args)
    {
        IsLoading = false;
        if (args is Exception ex)
        {
            ExtensionHost.LogMessage(new LogMessage() { Message = $"Error in saving search: {ex.Message}, {ex.StackTrace}" });
            SetStatusMessage(ex.InnerException is Octokit.ApiException oApiEx ? $"Error in saving search: {oApiEx.Message}" : $"Error in saving search: {ex.Message}", MessageState.Error);
            ExtensionHost.ShowStatus(_saveSearchStatusMessage);
        }
        else if (args is string message)
        {
            SetStatusMessage(message, MessageState.Info);
            ToastStatusMessage();
        }
        else
        {
            SetStatusMessage("Search saved successfully!", MessageState.Success);
            ToastStatusMessage();
        }
    }

    private void SetStatusMessage(string message, MessageState state)
    {
        _saveSearchStatusMessage.Message = message;
        _saveSearchStatusMessage.State = state;
    }

    private void ToastStatusMessage()
    {
        var toast = new ToastStatusMessage(_saveSearchStatusMessage);
        toast.Show();
    }

    private void OnLoadingChanged(object sender, bool isLoading)
    {
        IsLoading = isLoading;
    }
}
