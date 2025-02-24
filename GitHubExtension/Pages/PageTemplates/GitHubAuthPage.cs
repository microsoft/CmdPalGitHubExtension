// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Forms;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Pages;

internal partial class GitHubAuthPage : GitHubContentPage
{
    private StatusMessage _statusMessage;

    private GitHubForm _gitHubForm;
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
        set => _gitHubForm = (SignInForm)value;
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

    public GitHubAuthPage(GitHubForm gitHubAuthForm, StatusMessage statusMessage, string successMessage, string errorMessage)
    {
        _gitHubForm = gitHubAuthForm;
        PageForm.LoadingStateChanged += OnLoadingStateChanged;
        PageForm.FormSubmitted += OnFormSubmit;
        _statusMessage = statusMessage;
        _successMessage = successMessage;
        _errorMessage = errorMessage;
    }
}
