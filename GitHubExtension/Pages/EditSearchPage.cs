// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Forms;
using GitHubExtension.PersistentData;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Pages;

internal sealed partial class EditSearchPage : GitHubFormPage
{
    private readonly Search _searchToEdit;

    private SaveSearchForm _saveSearchForm;

    private StatusMessage _statusMessage;

    private string _successMessage;

    private string _errorMessage;

    public override StatusMessage StatusMessage { get => _statusMessage; set => _statusMessage = value; }

    public override string SuccessMessage { get => _successMessage; set => _successMessage = value; }

    public override string ErrorMessage { get => _errorMessage; set => _errorMessage = value; }

    public override GitHubForm PageForm { get => _saveSearchForm; set => _saveSearchForm = (SaveSearchForm)value; }

    public EditSearchPage(Search searchToEdit)
    {
        Icon = new IconInfo("\ue70f");
        Name = "Edit Search";
        _searchToEdit = searchToEdit;
        _saveSearchForm = new SaveSearchForm(_searchToEdit);
        _saveSearchForm.FormSubmitted += OnFormSubmit;
        _saveSearchForm.LoadingStateChanged += OnLoadingStateChanged;
        _statusMessage = new StatusMessage();
        _errorMessage = "Error in editing search";
        _successMessage = "Search edited successfully!";
    }

    public EditSearchPage(Search searchToEdit, SaveSearchForm saveSearchForm, StatusMessage statusMessage, string successMessage, string errorMessage)
    {
        _searchToEdit = searchToEdit;
        _saveSearchForm = saveSearchForm;
        _saveSearchForm.FormSubmitted += OnFormSubmit;
        _saveSearchForm.LoadingStateChanged += OnLoadingStateChanged;
        _statusMessage = statusMessage;
        _successMessage = successMessage;
        _errorMessage = errorMessage;
        StatusMessage = statusMessage;
        SuccessMessage = successMessage;
        ErrorMessage = errorMessage;
    }
}
