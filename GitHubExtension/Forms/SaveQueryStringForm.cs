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

internal sealed partial class SaveQueryStringForm : Form
{
    public static event TypedEventHandler<object, object?>? QuerySaved;

    internal event TypedEventHandler<object, bool>? LoadingStateChanged;

    public override ICommandResult SubmitForm(string payload)
    {
        try
        {
            LoadingStateChanged?.Invoke(this, true);

            Task.Run(() => HandleSubmit(payload));

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
        var path = Path.Combine(AppContext.BaseDirectory, GitHubHelper.GetTemplatePath("SaveQueryString"));
        var template = File.ReadAllText(path, Encoding.Default) ?? throw new FileNotFoundException(path);

        return template;
    }

    public string GetDataJsonFromFile()
    {
        var path = Path.Combine(AppContext.BaseDirectory, GitHubHelper.GetTemplatePath("SaveQueryStringData"));
        var data = File.ReadAllText(path, Encoding.Default) ?? throw new FileNotFoundException(path);
        return data;
    }

    private void HandleSubmit(string payload)
    {
        var query = GetQuery(payload);
        ExtensionHost.LogMessage(new LogMessage() { Message = $"Query: {query}" });
    }

    private Query GetQuery(string payload)
    {
        var query = new Query();
        var queryStr = string.Empty;
        try
        {
            var payloadJson = JsonNode.Parse(payload);

            if (payloadJson == null)
            {
                throw new InvalidOperationException("No query found");
            }

            if (payloadJson != null)
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                queryStr = payloadJson["EnteredQuery"].ToString() ?? string.Empty;
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                query = new Query(queryStr);
            }

            QuerySaved?.Invoke(this, query);
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
}
