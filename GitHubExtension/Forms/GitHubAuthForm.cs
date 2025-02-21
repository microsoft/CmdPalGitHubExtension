// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;
using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Foundation;

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

    public override string TemplateJson() => LoadTemplateJsonFromFile("SignIn");

    public override string StateJson() => "{}";

    public override void HandleSubmit(string payload)
    {
        try
        {
            Task.Run(() => HandleSignIn());
        }
        catch (Exception ex)
        {
            SignInAction?.Invoke(this, new SignInStatusChangedEventArgs(false, ex));
        }
    }

    private async Task HandleSignIn()
    {
        var authProvider = DeveloperIdProvider.GetInstance();

        var numPreviousDevIds = authProvider.GetLoggedInDeveloperIdsInternal().Count();

        await authProvider.LoginNewDeveloperIdAsync();

        var devIds = authProvider.GetLoggedInDeveloperIdsInternal();
        var numDevIds = devIds.Count();

        SignInAction?.Invoke(this, new SignInStatusChangedEventArgs(numDevIds > numPreviousDevIds, null));
    }
}
