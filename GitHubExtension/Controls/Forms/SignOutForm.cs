// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Controls.Commands;
using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Forms;

public partial class SignOutForm : FormContent
{
    private readonly IResources _resources;
    private readonly SignOutCommand _signOutCommand;
    private readonly AuthenticationMediator _authenticationMediator;
    private bool _isButtonEnabled = true;

    public SignOutForm(IResources resources, AuthenticationMediator authenticationMediator, SignOutCommand signOutCommand)
    {
        _resources = resources;
        _signOutCommand = signOutCommand;
        _authenticationMediator = authenticationMediator;
        _authenticationMediator.LoadingStateChanged += OnLoadingStateChanged;
        _authenticationMediator.SignInAction += ResetButton;
        _authenticationMediator.SignOutAction += ResetButton;
    }

    private void ResetButton(object? sender, SignInStatusChangedEventArgs e)
    {
        SetButtonEnabled(e.IsSignedIn);
    }

    private void OnLoadingStateChanged(object? sender, bool isLoading)
    {
        if (isLoading)
        {
            SetButtonEnabled(false);
        }
    }

    private void SetButtonEnabled(bool isEnabled)
    {
        _isButtonEnabled = isEnabled;
        TemplateJson = TemplateHelper.LoadTemplateJsonFromTemplateName("AuthTemplate", TemplateSubstitutions);
        OnPropertyChanged(nameof(TemplateJson));
    }

    public Dictionary<string, string> TemplateSubstitutions => new()
    {
        { "{{AuthTitle}}", _resources.GetResource("Forms_Sign_Out_Title") },
        { "{{AuthButtonTitle}}", _resources.GetResource("Forms_Sign_Out_Button_Title") },
        { "{{AuthIcon}}", $"data:image/png;base64,{GitHubIcon.GetBase64Icon(GitHubIcon.LogoWithBackplatePath)}" },
        { "{{AuthButtonTooltip}}", _resources.GetResource("Forms_Sign_Out_Tooltip") },
        { "{{ButtonIsEnabled}}", "true" },
    };

    public override string TemplateJson => TemplateHelper.LoadTemplateJsonFromTemplateName("AuthTemplate", TemplateSubstitutions);

    public override ICommandResult SubmitForm(string inputs, string data)
    {
        return _signOutCommand.Invoke();
    }
}
