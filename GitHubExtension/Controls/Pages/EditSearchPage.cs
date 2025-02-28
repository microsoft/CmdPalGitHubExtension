// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls.Forms;
using GitHubExtension.Controls.Pages.PageTemplates;
using GitHubExtension.Forms.Templates;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Pages;

internal sealed partial class EditSearchPage : GitHubContentPage
{
    private readonly ISearch _searchToEdit;

    private SaveSearchForm _saveSearchForm;

    private StatusMessage _statusMessage;

    private string _successMessage;

    private string _errorMessage;

    public override StatusMessage StatusMessage { get => _statusMessage; set => _statusMessage = value; }

    public override string SuccessMessage { get => _successMessage; set => _successMessage = value; }

    public override string ErrorMessage { get => _errorMessage; set => _errorMessage = value; }

    public override GitHubForm PageForm { get => _saveSearchForm; set => _saveSearchForm = (SaveSearchForm)value; }

    public EditSearchPage(ISearch searchToEdit, SaveSearchForm saveSearchForm, StatusMessage statusMessage, string successMessage, string errorMessage)
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
        Title = "Edit";
        Name = "Edit"; // Title is for the Page, Name is for the Command
        Icon = new IconInfo("\ue70f");
     }
}
