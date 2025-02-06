﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;
using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Foundation;

namespace GitHubExtension.Forms;

internal sealed partial class GitHubAuthForm : Form
{
    public static event EventHandler<SignInStatusChangedEventArgs>? SignInAction;

    public static event TypedEventHandler<object, bool>? LoadingStateChanged;

    public override string TemplateJson()
    {
        var path = Path.Combine(AppContext.BaseDirectory, GitHubHelper.GetTemplatePath("SignIn"));
        var template = File.ReadAllText(path, Encoding.Default) ?? throw new FileNotFoundException(path);

        template = Resources.ReplaceIdentifiers(template, Resources.GetWidgetResourceIdentifiers());
        var gh_base64 = GitHubIcon.GetBase64Icon("logo");
        template = template.Replace("%GitHubLogo%", gh_base64);

        return template;
    }

    public override string StateJson() => "{}";

    public override CommandResult SubmitForm(string payload)
    {
        LoadingStateChanged?.Invoke(this, true);

        Task.Run(async () => await HandleSignIn());

        return CommandResult.KeepOpen();
    }

    private async Task HandleSignIn()
    {
        try
        {
            var authProvider = DeveloperIdProvider.GetInstance();

            var numPreviousDevIds = authProvider.GetLoggedInDeveloperIdsInternal().Count();

            await authProvider.LoginNewDeveloperIdAsync();

            var devIds = authProvider.GetLoggedInDeveloperIdsInternal();
            var numDevIds = devIds.Count();

            SignInAction?.Invoke(this, new SignInStatusChangedEventArgs(numDevIds > numPreviousDevIds, null));
        }
        catch (Exception ex)
        {
            SignInAction?.Invoke(this, new SignInStatusChangedEventArgs(false, ex));
        }
    }
}
