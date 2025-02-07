// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;
using System.Text.Json.Nodes;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Foundation;

namespace GitHubExtension.Forms;

internal sealed partial class SaveQueryForm : Form
{
    internal event TypedEventHandler<object, object?>? QuerySaved;

    internal event TypedEventHandler<object, bool>? LoadingStateChanged;

    public override ICommandResult SubmitForm(string payload)
    {
        try
        {
            LoadingStateChanged?.Invoke(this, true);

            Task.Run(async () => await HandleSubmit(payload));

            return CommandResult.KeepOpen();
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage(new LogMessage() { Message = $"Error in SubmitForm: {ex.Message}" });
            return CommandResult.GoHome();
        }
    }

    public string GetTemplateJsonFromFile()
    {
        var path = Path.Combine(AppContext.BaseDirectory, GitHubHelper.GetTemplatePath("SaveQuery"));
        var template = File.ReadAllText(path, Encoding.Default) ?? throw new FileNotFoundException(path);

        return template;
    }

    public string GetDataJsonFromFile()
    {
        var path = Path.Combine(AppContext.BaseDirectory, GitHubHelper.GetTemplatePath("SaveQueryData"));
        var data = File.ReadAllText(path, Encoding.Default) ?? throw new FileNotFoundException(path);
        return data;
    }

    private async Task HandleSubmit(string payload)
    {
        var queryUrl = GetQueryUrl(payload);
        await Task.Delay(2000); // force a delay to satisfy compiler
        ExtensionHost.LogMessage(new LogMessage() { Message = $"Query URL: {queryUrl}" });
    }

    private string GetQueryUrl(string payload)
    {
        var queryUrl = string.Empty;
        try
        {
            var payloadJson = JsonNode.Parse(payload);

            if (payloadJson == null)
            {
                throw new InvalidOperationException("No query found");
            }

            if (payloadJson != null)
            {
                queryUrl = payloadJson["queryUrl"]?.ToString();
                if (string.IsNullOrEmpty(queryUrl))
                {
                    throw new InvalidOperationException("No query URL found");
                }
            }

            QuerySaved?.Invoke(this, queryUrl);
            return queryUrl;
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage(new LogMessage() { Message = $"Error in GetQueryUrl: {ex.Message}" });
            QuerySaved?.Invoke(this, ex);
        }

        return string.Empty;
    }

    public override string DataJson()
    {
        return GetDataJsonFromFile();
    }

    public override string TemplateJson()
    {
        return GetTemplateJsonFromFile();
    }

    public string GetTemplateJsonFromString()
    {
        var gh_base64 = GitHubIcon.GetBase64Icon("logo");
        var template = $@"
        {{
            ""type"": ""AdaptiveCard"",
            ""version"": ""1.3"",
            ""body"": [
                {{
                    ""type"": ""Image"",
                    ""url"": ""data:image/png;base64,%GitHubLogo%"",
                    ""size"": ""large"",
                    ""horizontalAlignment"": ""center""
                }},
                {{
                    ""type"": ""Container"",
                    ""items"": [
                        {{
                            ""type"": ""TextBlock"",
                            ""text"": ""Query URL"",
                            ""weight"": ""bolder"",
                            ""size"": ""medium""
                        }},
                        {{
                            ""type"": ""Input.Text"",
                            ""id"": ""queryUrl"",
                            ""placeholder"": ""Enter query URL""
                        }}
                    ],
                    ""horizontalAlignment"": ""left""
                }}
            ],
            ""actions"": [
                {{
                    ""type"": ""Action.Submit"",
                    ""title"": ""Submit"",
                    ""data"": {{
                        ""queryUrl"": ""{{queryUrl.value}}""
                    }}
                }}
            ]
        }}";
        template = template.Replace("%GitHubLogo%", gh_base64);
        return template;
    }

    private sealed class Payload
    {
        public string QueryUrl { get; set; } = string.Empty;
    }
}
