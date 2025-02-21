// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GitHubExtension.Forms;

internal sealed partial class GitHubAuthForm : GitHubForm
{
    public static event EventHandler<SignInStatusChangedEventArgs>? SignInAction;

    public override Dictionary<string, string> TemplateSubstitutions => new()
    {
        { "{{AuthTitle}}", "Sign In" },
        { "{{AuthButtonTitle}}", "Sign In" },
        { "{{AuthIcon}}", $"data:image/png;base64,{GitHubIcon.GetBase64Icon("logo")}" },
        { "{{AuthButtonTooltip}}", "Sign in to GitHub" },
    };

    public override ICommandResult DefaultSubmitFormCommand => CommandResult.KeepOpen();

    public override string TemplateJson() => LoadTemplateJsonFromFile("AuthTemplate");

    public override void HandleSubmit(string payload)
    {
        try
        {
            var signInSucceeded = HandleSignIn().Result;
            RaiseLoadingStateChanged(false);
            SignInAction?.Invoke(this, new SignInStatusChangedEventArgs(signInSucceeded, null));
            RaiseFormSubmitted(new FormSubmitEventArgs(signInSucceeded, null));
        }
        catch (Exception ex)
        {
            RaiseLoadingStateChanged(false);
            SignInAction?.Invoke(this, new SignInStatusChangedEventArgs(false, ex));
        }
    }

    private async Task<bool> HandleSignIn()
    {
        var authProvider = DeveloperIdProvider.GetInstance();

        var numPreviousDevIds = authProvider.GetLoggedInDeveloperIdsInternal().Count();

        await authProvider.LoginNewDeveloperIdAsync();

        var numDevIds = authProvider.GetLoggedInDeveloperIdsInternal().Count();

        return numDevIds > numPreviousDevIds;
    }
}
