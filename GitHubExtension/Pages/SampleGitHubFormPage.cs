// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Forms;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Pages;

internal sealed partial class SampleGitHubFormPage : GitHubFormPage
{
    private StatusMessage _statusMessage;

    private GitHubForm _gitHubForm;

    public override string Title => "Sample GitHub Form Page";

    public override string SuccessMessage => "Sample success message";

    public override string ErrorMessage => "Sample error message";

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

    public SampleGitHubFormPage()
    {
        _gitHubForm = new SampleGitHubForm();
        PageForm.LoadingStateChanged += OnLoadingStateChanged;
        PageForm.FormSubmitted += OnFormSubmit;
        _statusMessage = new StatusMessage { Message = "Sample status message" };
    }
}
