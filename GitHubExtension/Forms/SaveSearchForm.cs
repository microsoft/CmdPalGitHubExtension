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

internal sealed partial class SaveSearchForm : Form
{
    public static event TypedEventHandler<object, object?>? SearchSaved;

    internal event TypedEventHandler<object, bool>? LoadingStateChanged;

    private readonly SearchInput _queryInput;

    public SaveSearchForm()
    {
        _queryInput = SearchInput.SearchString; // default
    }

    public SaveSearchForm(SearchInput input)
    {
        _queryInput = input;
    }

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
            ExtensionHost.LogMessage(new LogMessage() { Message = $"Error in SubmitForm: {ex.Message}, {ex.InnerException}" });
            return CommandResult.GoHome();
        }
    }

    public string GetTemplateJsonFromFile()
    {
        var templateName = _queryInput == SearchInput.SearchString ? "SaveSearch" : "SaveSearchSurvey";
        var path = Path.Combine(AppContext.BaseDirectory, GitHubHelper.GetTemplatePath(templateName));
        var template = File.ReadAllText(path, Encoding.Default) ?? throw new FileNotFoundException(path);

        return template;
    }

    public string GetDataJsonFromFile()
    {
        var dataName = _queryInput == SearchInput.SearchString ? "SaveSearchData" : "SaveSearchSurveyData";
        var path = Path.Combine(AppContext.BaseDirectory, GitHubHelper.GetTemplatePath(dataName));
        var data = File.ReadAllText(path, Encoding.Default) ?? throw new FileNotFoundException(path);
        return data;
    }

    private void HandleSubmit(string payload)
    {
        var query = GetSearch(payload);
        ExtensionHost.LogMessage(new LogMessage() { Message = $"Search: {query}" });
    }

    private Search GetSearch(string payload)
    {
        var query = new Search();
        var queryStr = string.Empty;
        try
        {
            var payloadJson = JsonNode.Parse(payload);

            if (payloadJson == null)
            {
                throw new InvalidOperationException("No query found");
            }

            if (payloadJson != null && _queryInput == SearchInput.SearchString)
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                queryStr = payloadJson["EnteredSearch"].ToString() ?? string.Empty;
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                var repoHelper = GitHubRepositoryHelper.Instance;
                repoHelper.ValidateSearch(queryStr).Wait();

                query = new Search(queryStr);
            }
            else if (payloadJson != null && _queryInput == SearchInput.Survey)
            {
                query = CreateSearchFromJson(payloadJson);
            }

            SearchSaved?.Invoke(this, query);
            return query;
        }
        catch (Exception ex)
        {
            SearchSaved?.Invoke(this, ex);
        }

        return query;
    }

    public static Search CreateSearchFromJson(JsonNode jsonNode)
    {
        if (jsonNode == null)
        {
            throw new InvalidOperationException("No query found");
        }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var query = new Search(
            jsonNode["name"] != null ? jsonNode["name"].ToString() : string.Empty,
            jsonNode["type"] != null ? jsonNode["type"].ToString() : string.Empty,
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

    public override string DataJson()
    {
        return GetDataJsonFromFile();
    }

    public override string TemplateJson()
    {
        return GetTemplateJsonFromFile();
    }
}
