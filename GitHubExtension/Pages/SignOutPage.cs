// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Pages;

internal sealed partial class SignOutPage : GitHubFormPage
{
    private StatusMessage _statusMessage;

    private string _successMessage;

    private string _errorMessage;

    private GitHubForm _gitHubForm;

    public override StatusMessage StatusMessage
    {
        get => _statusMessage;
        set => _statusMessage = value;
    }

    public override GitHubForm PageForm
    {
        get => _gitHubForm;
        set => _gitHubForm = value;
    }

    public override string SuccessMessage
    {
        get => _successMessage;
        set => _successMessage = value;
    }

    public override string ErrorMessage
    {
        get => _errorMessage;
        set => _errorMessage = value;
    }

    public SignOutPage()
    {
        _gitHubForm = new SignOutForm();
        PageForm.LoadingStateChanged += OnLoadingStateChanged;
        PageForm.FormSubmitted += OnFormSubmit;
        _statusMessage = new();
        _successMessage = "Sign out succeeded!";
        _errorMessage = "Sign out failed";
    }
}
