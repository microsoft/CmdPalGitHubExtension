// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using GitHubExtension.Controls.Commands;
using GitHubExtension.DeveloperIds;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Forms;

public partial class SignOutForm : FormContent
{
    private readonly IResources _resources;
    private readonly SignOutCommand _signOutCommand;
    private readonly AuthenticationMediator _authenticationMediator;
    private readonly IDeveloperIdProvider _developerIdProvider;
    private bool _isButtonEnabled = true;

    private string IsButtonEnabled =>
    _isButtonEnabled.ToString(CultureInfo.InvariantCulture).ToLower(CultureInfo.InvariantCulture);

    public SignOutForm(IResources resources, AuthenticationMediator authenticationMediator, SignOutCommand signOutCommand, IDeveloperIdProvider developerIdProvider)
    {
        _resources = resources;
        _signOutCommand = signOutCommand;
        _authenticationMediator = authenticationMediator;
        _developerIdProvider = developerIdProvider;
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
        { "{{AuthButtonTitle}}", $"{_resources.GetResource("Forms_Sign_Out_Button_Title")} {_developerIdProvider.GetLoggedInDeveloperId()?.LoginId ?? string.Empty}" },
        { "{{AuthIcon}}", $"data:image/png;base64,{GitHubIcon.GetBase64Icon(GitHubIcon.LogoWithBackplatePath)}" },
        { "{{AuthButtonTooltip}}", _resources.GetResource("Forms_Sign_Out_Tooltip") },
        { "{{ButtonIsEnabled}}", IsButtonEnabled },
    };

    public override string TemplateJson => TemplateHelper.LoadTemplateJsonFromTemplateName("AuthTemplate", TemplateSubstitutions);

    public override ICommandResult SubmitForm(string inputs, string data)
    {
        return _signOutCommand.Invoke();
    }
}
