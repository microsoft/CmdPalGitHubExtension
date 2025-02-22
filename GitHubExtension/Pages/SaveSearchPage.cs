// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Forms;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Pages;

internal sealed partial class SaveSearchPage : GitHubFormPage
{
    private readonly SearchInput _searchInput;

    private SaveSearchForm _saveSearchForm;

    private StatusMessage _statusMessage;

    private string _successMessage;

    private string _errorMessage;

    public override StatusMessage StatusMessage { get => _statusMessage; set => _statusMessage = value; }

    public override string SuccessMessage { get => _successMessage; set => _successMessage = value; }

    public override string ErrorMessage { get => _errorMessage; set => _errorMessage = value; }

    public override GitHubForm PageForm { get => _saveSearchForm; set => _saveSearchForm = (SaveSearchForm)value; }

    public SaveSearchPage()
        : this(SearchInput.SearchString)
    {
        _saveSearchForm = new SaveSearchForm();
        _saveSearchForm.FormSubmitted += OnFormSubmit;
        _saveSearchForm.LoadingStateChanged += OnLoadingStateChanged;
        _statusMessage = new StatusMessage();
        _successMessage = "Search saved successfully!";
        _errorMessage = "Error in saving search";
    }

    public SaveSearchPage(SearchInput input)
    {
        _searchInput = input;
        _saveSearchForm = new SaveSearchForm(_searchInput);
        _saveSearchForm.FormSubmitted += OnFormSubmit;
        _saveSearchForm.LoadingStateChanged += OnLoadingStateChanged;
        _statusMessage = new StatusMessage();
        _successMessage = $"Search saved successfully!";
        _errorMessage = "Error in saving search";
    }
}
