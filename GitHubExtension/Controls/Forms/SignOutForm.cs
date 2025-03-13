// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Foundation;

namespace GitHubExtension.Controls.Forms;

public sealed partial class SignOutForm : FormContent, IGitHubForm
{
    public static event EventHandler<SignInStatusChangedEventArgs>? SignOutAction;

    public event TypedEventHandler<object, bool>? LoadingStateChanged;

    public event TypedEventHandler<object, FormSubmitEventArgs>? FormSubmitted;

    private readonly IDeveloperIdProvider _developerIdProvider;

    public SignOutForm(IDeveloperIdProvider developerIdProvider)
    {
        _developerIdProvider = developerIdProvider;
    }

    public Dictionary<string, string> TemplateSubstitutions => new()
    {
        { "{{AuthTitle}}", "Are you sure you want to sign out?" },
        { "{{AuthButtonTitle}}", "Sign out" },
        { "{{AuthIcon}}", $"data:image/png;base64,{GitHubIcon.GetBase64Icon("logo")}" },
        { "{{AuthButtonTooltip}}", "Sign out GitHub extension" },
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
