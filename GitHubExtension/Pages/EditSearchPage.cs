// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Forms;
using GitHubExtension.PersistentData;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Pages;

internal sealed partial class EditSearchPage : FormPage
{
    private readonly Search _searchToEdit;

    private readonly StatusMessage _editSearchStatusMessage;

    private readonly IFormFactory _formFactory;

    public EditSearchPage(Search searchToEdit, IFormFactory formFactory)
    {
        Icon = new IconInfo("\ue70f");
        Name = "Edit Search";
        _searchToEdit = searchToEdit;
        _editSearchStatusMessage = new StatusMessage();
        SaveSearchForm.SearchSaved += OnSearchSaved;
        SaveSearchForm.SearchSaving += OnSearchSaving;
        _formFactory = formFactory;
    }

    public override IForm[] Forms()
    {
        return new IForm[] { _formFactory.GetSaveSearchForm(_searchToEdit) };
    }

    private void OnSearchSaved(object sender, object? args)
    {
        IsLoading = false;
        if (args is Exception ex)
        {
            ExtensionHost.LogMessage(new LogMessage() { Message = $"Error in saving search: {ex.Message}, {ex.StackTrace}" });
            SetStatusMessage(ex.InnerException is Octokit.ApiException oApiEx ? $"Error in saving search: {oApiEx.Message}" : $"Error in saving search: {ex.Message}", MessageState.Error);
            ExtensionHost.ShowStatus(_editSearchStatusMessage);
        }
        else
        {
            SetStatusMessage("Search saved successfully!", MessageState.Success);
            ToastStatusMessage();
        }
    }

    private void SetStatusMessage(string message, MessageState state)
    {
        _editSearchStatusMessage.Message = message;
        _editSearchStatusMessage.State = state;
    }

    private void ToastStatusMessage()
    {
        var toast = new ToastStatusMessage(_editSearchStatusMessage);
        toast.Show();
    }

    private void OnSearchSaving(object sender, bool isLoading)
    {
        IsLoading = isLoading;
    }
}
