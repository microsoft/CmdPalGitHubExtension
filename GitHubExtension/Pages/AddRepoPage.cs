// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Forms;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Pages;

internal sealed partial class AddRepoPage : GitHubFormPage
{
    private AddRepoForm _addRepoForm;

    private StatusMessage _statusMessage;

    private string _successMessage;

    private string _errorMessage;

    public override StatusMessage StatusMessage { get => _statusMessage; set => _statusMessage = value; }

    public override string SuccessMessage { get => _successMessage; set => _successMessage = value; }

    public override string ErrorMessage { get => _errorMessage; set => _errorMessage = value; }

    public override GitHubForm PageForm { get => _addRepoForm; set => _addRepoForm = (AddRepoForm)value; }

    public AddRepoPage()
    {
        _addRepoForm = new();
        _addRepoForm.FormSubmitted += OnFormSubmit;
        _addRepoForm.LoadingStateChanged += OnLoadingStateChanged;
        _statusMessage = new StatusMessage();
        _successMessage = "Repository added successfully!";
        _errorMessage = "Error in adding repository";
    }
}
