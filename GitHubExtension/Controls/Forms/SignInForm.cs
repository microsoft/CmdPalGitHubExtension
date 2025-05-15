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

public partial class SignInForm : FormContent
{
    private readonly IDeveloperIdProvider _developerIdProvider;
    private readonly IResources _resources;
    private readonly AuthenticationMediator _authenticationMediator;
    private readonly SignInCommand _signInCommand;

    private bool _isButtonEnabled = true;

    private string IsButtonEnabled =>
        _isButtonEnabled.ToString(CultureInfo.InvariantCulture).ToLower(CultureInfo.InvariantCulture);

    public SignInForm(AuthenticationMediator authenticationMediator, IResources resources, IDeveloperIdProvider developerIdProvider, SignInCommand signInCommand)
    {
        _authenticationMediator = authenticationMediator;
        _developerIdProvider = developerIdProvider;
        _authenticationMediator.LoadingStateChanged += OnLoadingStateChanged;
        _developerIdProvider.OAuthRedirected += DeveloperIdProvider_OAuthRedirected;
        _authenticationMediator.SignInAction += ResetButton;
        _authenticationMediator.SignOutAction += ResetButton;
        _resources = resources;
        _signInCommand = signInCommand;
    }

    private void ResetButton(object? sender, SignInStatusChangedEventArgs e)
    {
        SetButtonEnabled(!e.IsSignedIn);
    }

    private void SignOutForm_SignOutAction(object? sender, SignInStatusChangedEventArgs e)
    {
        _isButtonEnabled = !e.IsSignedIn;
    }

    private void DeveloperIdProvider_OAuthRedirected(object? sender, Exception? e)
    {
        if (e is not null)
        {
            SetButtonEnabled(true);
            _authenticationMediator.SetLoadingState(false);
            _authenticationMediator.SignIn(new SignInStatusChangedEventArgs(false, e));
            return;
        }

        SetButtonEnabled(false);
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
        { "{{AuthTitle}}", _resources.GetResource("Forms_Sign_In") },
        { "{{AuthButtonTitle}}", _resources.GetResource("Forms_Sign_In") },
        { "{{AuthIcon}}", $"data:image/png;base64,{GitHubIcon.GetBase64Icon(GitHubIcon.LogoWithBackplatePath)}" },
        { "{{AuthButtonTooltip}}", _resources.GetResource("Forms_Sign_In_Tooltip") },
        { "{{ButtonIsEnabled}}", IsButtonEnabled },
    };

    public override string TemplateJson => TemplateHelper.LoadTemplateJsonFromTemplateName("AuthTemplate", TemplateSubstitutions);

    public override ICommandResult SubmitForm(string inputs, string data)
    {
        return _signInCommand.Invoke();
    }
}
