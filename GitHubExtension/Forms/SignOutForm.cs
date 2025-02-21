// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension;
using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

internal sealed partial class SignOutForm : GitHubForm
{
    public override Dictionary<string, string> TemplateSubstitutions => new()
    {
        { "{{AuthTitle}}", "Are you sure you want to sign out?" },
        { "{{AuthButtonTitle}}", "Sign out" },
        { "{{AuthIcon}}", $"data:image/png;base64,{GitHubIcon.GetBase64Icon("logo")}" },
        { "{{AuthButtonTooltip}}", "Sign out GitHub extension" },
    };

    public override ICommandResult DefaultSubmitFormCommand => CommandResult.GoHome();

    public static event EventHandler<SignInStatusChangedEventArgs>? SignOutAction;

    public override string TemplateJson() => LoadTemplateJsonFromFile("AuthTemplate");

    public override void HandleSubmit(string payload)
    {
        try
        {
            var authProvider = DeveloperIdProvider.GetInstance();
            var devIds = authProvider.GetLoggedInDeveloperIdsInternal();

            foreach (var devId in devIds)
            {
                authProvider.LogoutDeveloperId(devId);
            }

            var signOutSucceeded = !authProvider.GetLoggedInDeveloperIdsInternal().Any();

            SignOutAction?.Invoke(this, new SignInStatusChangedEventArgs(!signOutSucceeded, null));
        }
        catch (Exception ex)
        {
            // if sign out fails, the user is still signed in (true)
            SignOutAction?.Invoke(this, new SignInStatusChangedEventArgs(true, ex));
        }
    }
}
