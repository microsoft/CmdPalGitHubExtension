// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Forms;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Pages;

internal sealed partial class SampleGitHubFormPage : GitHubFormPage
{
    public override string Title => "Sample GitHub Form Page";

    public override StatusMessage StatusMessage => new() { Message = "Sample status message" };

    public override string SuccessMessage => "Sample success message";

    public override string ErrorMessage => "Sample error message";

    public SampleGitHubFormPage()
    {
        PageForm = new SampleGitHubForm();
        PageForm.LoadingStateChanged += OnLoadingStateChanged;
        PageForm.FormSubmitted += OnFormSubmit;
    }
}
