﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Foundation;

namespace GitHubExtension.Controls.Forms;

public sealed partial class SignInForm : FormContent, IGitHubForm
{
    public static event EventHandler<SignInStatusChangedEventArgs>? SignInAction;

    public event TypedEventHandler<object, bool>? LoadingStateChanged;

    public event TypedEventHandler<object, FormSubmitEventArgs>? FormSubmitted;

    private readonly IDeveloperIdProvider _developerIdProvider;

    public SignInForm(IDeveloperIdProvider developerIdProvider)
    {
        _developerIdProvider = developerIdProvider;
    }

    public Dictionary<string, string> TemplateSubstitutions => new()
    {
        { "{{AuthTitle}}", "Sign In" },
        { "{{AuthButtonTitle}}", "Sign In" },
        { "{{AuthIcon}}", $"data:image/png;base64,{GitHubIcon.GetBase64Icon("logo")}" },
        { "{{AuthButtonTooltip}}", "Sign in to GitHub" },
    };

    public override string TemplateJson => TemplateHelper.LoadTemplateJsonFromTemplateName("AuthTemplate", TemplateSubstitutions);

    public override ICommandResult SubmitForm(string inputs, string data)
    {
        LoadingStateChanged?.Invoke(this, true);
        Task.Run(() =>
        {
            try
            {
                var signInSucceeded = HandleSignIn().Result;
                LoadingStateChanged?.Invoke(this, false);
                SignInAction?.Invoke(this, new SignInStatusChangedEventArgs(signInSucceeded, null));
                FormSubmitted?.Invoke(this, new FormSubmitEventArgs(signInSucceeded, null));
            }
            catch (Exception ex)
            {
                LoadingStateChanged?.Invoke(this, false);
                SignInAction?.Invoke(this, new SignInStatusChangedEventArgs(false, ex));
                FormSubmitted?.Invoke(this, new FormSubmitEventArgs(false, ex));
            }
        });
        return CommandResult.KeepOpen();
    }

    private async Task<bool> HandleSignIn()
    {
        var numPreviousDevIds = _developerIdProvider.GetLoggedInDeveloperIdsInternal().Count();

        await _developerIdProvider.LoginNewDeveloperIdAsync();

        var numDevIds = _developerIdProvider.GetLoggedInDeveloperIdsInternal().Count();

        return numDevIds > numPreviousDevIds;
    }
}
