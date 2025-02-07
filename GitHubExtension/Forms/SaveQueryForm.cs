// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;
using System.Text.Json.Nodes;
using GitHubExtension.DataModel.DataObjects;
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
        var query = GetQuery(payload);
        await Task.Delay(2000); // force a delay to satisfy compiler
        ExtensionHost.LogMessage(new LogMessage() { Message = $"Query: {query.ToString()}" });
    }

    private Query GetQuery(string payload)
    {
        var query = new Query();
        try
        {
            var payloadJson = JsonNode.Parse(payload);

            if (payloadJson == null)
            {
                throw new InvalidOperationException("No query found");
            }

            if (payloadJson != null)
            {
                query = CreateQueryFromJson(payloadJson);
            }

            QuerySaved?.Invoke(this, query.ToString());
            return query;
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage(new LogMessage() { Message = $"Error in GetQueryUrl: {ex.Message}" });
            QuerySaved?.Invoke(this, ex);
        }

        return query;
    }

    public override string DataJson()
    {
        return GetDataJsonFromFile();
    }

    public override string TemplateJson()
    {
        return GetTemplateJsonFromFile();
    }

    public static Query CreateQueryFromJson(JsonNode jsonNode)
    {
        if (jsonNode == null)
        {
            throw new InvalidOperationException("No query found");
        }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var query = new Query(
            jsonNode["owner"] != null ? jsonNode["owner"].ToString() : string.Empty,
            jsonNode["repository"] != null ? jsonNode["repository"].ToString() : string.Empty,
            jsonNode["dateCreated"] != null ? jsonNode["dateCreated"].ToString() : string.Empty,
            jsonNode["language"] != null ? jsonNode["language"].ToString() : string.Empty,
            jsonNode["state"] != null ? jsonNode["state"].ToString() : string.Empty,
            jsonNode["reason"] != null ? jsonNode["reason"].ToString() : string.Empty,
            jsonNode["numberOfComments"] != null ? jsonNode["numberOfComments"].ToString() : string.Empty,
            jsonNode["labels"] != null ? jsonNode["labels"].ToString() : string.Empty,
            jsonNode["author"] != null ? jsonNode["author"].ToString() : string.Empty,
            jsonNode["mentionedUsers"] != null ? jsonNode["mentionedUsers"].ToString() : string.Empty,
            jsonNode["assignees"] != null ? jsonNode["assignees"].ToString() : string.Empty,
            jsonNode["updatedDate"] != null ? jsonNode["updatedDate"].ToString() : string.Empty);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        return query;
    }
}
