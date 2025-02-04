// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;
using System.Text.Json.Nodes;
using GitHubExtension.DeveloperId;
using GitHubExtension.Helpers;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using Windows.Foundation;

internal sealed partial class SignOutForm : Form
{
    public static event EventHandler<SignInStatusChangedEventArgs>? SignOutAction;

    public static event TypedEventHandler<object, bool>? LoadingStateChanged;

    public override string TemplateJson()
    {
        var template = $$"""
        {
          "type": "AdaptiveCard",
          "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
          "version": "1.5",
          "body": [
            {
              "type": "Container",
              "spacing": "none",
              "items": [
                {
                  "type": "Image",
                  "url": "data:image/png;base64,%GitHubLogo%",
                  "horizontalAlignment": "center",
                  "size": "large"
                },
                {
                  "type": "TextBlock",
                  "text": "Are you sure you want to sign out?",
                  "wrap": true,
                  "horizontalAlignment": "center",
                  "height": "stretch",
                  "size": "medium",
                  "weight": "bolder"
                },
                {
                  "type": "ColumnSet",
                  "columns": [
                    {
                      "type": "Column",
                      "width": "stretch"
                    },
                    {
                      "type": "Column",
                      "width": "auto",
                      "items": [
                        {
                          "type": "ActionSet",
                          "actions": [
                            {
                              "type": "Action.Submit",
                              "title": "Sign out"
                            }
                          ]
                        }
                      ]
                    },
                    {
                      "type": "Column",
                      "width": "stretch"
                    }
                  ]
                }
              ]
            }
          ]
        }
        """;

        template = Resources.ReplaceIdentifiers(template, Resources.GetWidgetResourceIdentifiers());
        var gh_base64 = GitHubIcon.GetBase64Icon("logo");
        template = template.Replace("%GitHubLogo%", gh_base64);

        return template;
    }

    public override CommandResult SubmitForm(string payload)
    {
        LoadingStateChanged?.Invoke(this, true);

        Task.Run(async () => await HandleSignOut());

        return CommandResult.KeepOpen();
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    private async Task HandleSignOut()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
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
