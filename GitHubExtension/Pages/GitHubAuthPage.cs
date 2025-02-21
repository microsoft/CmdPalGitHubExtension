// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Forms;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Pages;

internal sealed partial class GitHubAuthPage : GitHubFormPage
{
    private StatusMessage _statusMessage;

    private GitHubAuthForm _gitHubForm;
    private string _successMessage;
    private string _errorMessage;

    public override StatusMessage StatusMessage
    {
        get => _statusMessage;
        set => _statusMessage = value;
    }

    public override GitHubForm PageForm
    {
        get => _gitHubForm;
        set => _gitHubForm = (GitHubAuthForm)value;
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

    public GitHubAuthPage()
    {
        _gitHubForm = new GitHubAuthForm();
        PageForm.LoadingStateChanged += OnLoadingStateChanged;
        PageForm.FormSubmitted += OnFormSubmit;
        _statusMessage = new();
        _successMessage = "Sign in succeeded!";
        _errorMessage = "Sign in failed";
    }
}
