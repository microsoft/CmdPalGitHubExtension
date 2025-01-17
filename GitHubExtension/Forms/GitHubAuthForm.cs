// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;
using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Windows.Foundation;

namespace GitHubExtension.Forms;

internal sealed partial class GitHubAuthForm : Form
{
    internal event TypedEventHandler<object, object?>? SignInAction;

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
        try
        {
            Task.Run(HandleSignIn).GetAwaiter().GetResult();

            // TODO: Prevent CmdPal from exiting while user is logging in
            var message = new StatusMessage() { Message = "Sign in succeeded!", State = MessageState.Success };
            ExtensionHost.Host?.ShowStatus(message);
            return CommandResult.KeepOpen();
        }
        catch (Exception ex)
        {
            var message = new StatusMessage() { Message = $"Error in sign-in: {ex}", State = MessageState.Error };
            ExtensionHost.Host?.ShowStatus(message);
            return CommandResult.KeepOpen();
        }
    }

    private async Task HandleSignIn()
    {
        var authProvider = DeveloperIdProvider.GetInstance();

        var numPreviousDevIds = authProvider.GetLoggedInDeveloperIdsInternal().Count();

        await authProvider.LoginNewDeveloperIdAsync();

        var numDevIds = authProvider.GetLoggedInDeveloperIdsInternal().Count();

        if (numDevIds > numPreviousDevIds)
        {
            SignInAction?.Invoke(this, null);
        }
    }
}
