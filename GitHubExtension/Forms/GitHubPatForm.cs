// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Nodes;
using GitHubExtension.Helpers;
using Microsoft.CmdPal.Extensions.Helpers;

namespace GitHubExtension.Forms;

internal sealed partial class GitHubPatForm : Form
{
    public override string TemplateJson()
    {
        var json = $$"""
                    {
                      "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                      "type": "AdaptiveCard",
                      "version": "1.5",
                      "body": [
                        {
                          "type": "Input.Text",
                          "style": "password",
                          "id": "pat",
                          "label": "PAT (personal access token)",
                          "isRequired": true,
                          "errorMessage": "PAT required"
                        }
                      ],
                      "actions": [
                        {
                          "type": "Action.Submit",
                          "title": "Save",
                          "data": {
                            "pat": "pat"
                          }
                        }
                      ]
                    }
                    """;
        return json;
    }

    public override string DataJson() => throw new NotImplementedException();

    public override string StateJson() => "{}";

    public override CommandResult SubmitForm(string payload)
    {
        var formInput = JsonNode.Parse(payload);
        if (formInput == null)
        {
            return CommandResult.GoHome();
        }

        // get the name and url out of the values
        var formPat = formInput["pat"] ?? string.Empty;

        // Construct a new json blob with the name and url
        var json = $$"""
                    {
                        "pat": "{{formPat}}"
                    }
                    """;

        File.WriteAllText(GitHubHelper.StateJsonPath(), json);
        return CommandResult.GoHome();
    }
}
