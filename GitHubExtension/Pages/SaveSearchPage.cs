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
    private readonly SaveSearchForm _saveSearchForm;

    private readonly SearchInput _queryInput;

#pragma warning disable IDE0044 // Add readonly modifier
    private StatusMessage _saveSearchStatusMessage;
#pragma warning restore IDE0044 // Add readonly modifier

    public SaveSearchPage()
    {
        _saveSearchForm = new();
        SaveSearchForm.SearchSaved += OnSearchSaved;
        _saveSearchForm.LoadingStateChanged += OnLoadingChanged;
        _saveSearchStatusMessage = new StatusMessage();
        ExtensionHost.HideStatus(_saveSearchStatusMessage);
        _queryInput = SearchInput.SearchString; // default
    }

    public SaveSearchPage(SearchInput input)
    {
        _queryInput = input;
        _saveSearchForm = new(input);
        SaveSearchForm.SearchSaved += OnSearchSaved;
        _saveSearchForm.LoadingStateChanged += OnLoadingChanged;
        _saveSearchStatusMessage = new StatusMessage();
        ExtensionHost.HideStatus(_saveSearchStatusMessage);
    }

    public override IForm[] Forms()
    {
        ExtensionHost.HideStatus(_saveSearchStatusMessage);
        return new IForm[] { _saveSearchForm };
    }

    private void OnSearchSaved(object sender, object? args)
    {
        IsLoading = false;
        if (args is Exception ex)
        {
            ExtensionHost.LogMessage(new LogMessage() { Message = $"Error in saving query: {ex.Message}, {ex.StackTrace}" });

            _saveSearchStatusMessage.Message = ex.InnerException is Octokit.ApiException oApiEx ? $"Error in saving query: {oApiEx.Message}" : $"Error in saving query: {ex.Message}";
            _saveSearchStatusMessage.State = MessageState.Error;
            ExtensionHost.ShowStatus(_saveSearchStatusMessage);
        }
        else if (args is string message)
        {
            _saveSearchStatusMessage.Message = message;
            _saveSearchStatusMessage.State = MessageState.Info;
            var toast = new ToastStatusMessage(_saveSearchStatusMessage);
            toast.Show();
        }
        else
        {
            _saveSearchStatusMessage.Message = "Search saved successfully!";
            _saveSearchStatusMessage.State = MessageState.Success;
            var toast = new ToastStatusMessage(_saveSearchStatusMessage);
            toast.Show();
        }
    }

    private void OnLoadingChanged(object sender, bool isLoading)
    {
        IsLoading = isLoading;
    }
}
