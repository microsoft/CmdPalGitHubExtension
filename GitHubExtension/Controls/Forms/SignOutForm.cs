// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Controls.Forms;

public sealed partial class SignOutForm : FormContent, IGitHubForm
{
    public static event EventHandler<SignInStatusChangedEventArgs>? SignOutAction;

    public event EventHandler<bool>? LoadingStateChanged;

    public event EventHandler<FormSubmitEventArgs>? FormSubmitted;

    private readonly IDeveloperIdProvider _developerIdProvider;
    private readonly IResources _resources;

    public SignOutForm(IDeveloperIdProvider developerIdProvider, IResources resources)
    {
        _developerIdProvider = developerIdProvider;
        _resources = resources;
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
        LoadingStateChanged?.Invoke(this, true);
        Task.Run(() =>
        {
            try
            {
                var devIds = _developerIdProvider.GetLoggedInDeveloperIdsInternal();

                foreach (var devId in devIds)
                {
                    _developerIdProvider.LogoutDeveloperId(devId);
                }

                var signOutSucceeded = !_developerIdProvider.GetLoggedInDeveloperIdsInternal().Any();

                LoadingStateChanged?.Invoke(this, false);
                SignOutAction?.Invoke(this, new SignInStatusChangedEventArgs(!signOutSucceeded, null));
                FormSubmitted?.Invoke(this, new FormSubmitEventArgs(true, null));
            }
            catch (Exception ex)
            {
                LoadingStateChanged?.Invoke(this, false);

                // if sign out fails, the user is still signed in (true)
                SignOutAction?.Invoke(this, new SignInStatusChangedEventArgs(true, ex));
                FormSubmitted?.Invoke(this, new FormSubmitEventArgs(false, ex));
            }
        });
        return CommandResult.KeepOpen();
    }
}
