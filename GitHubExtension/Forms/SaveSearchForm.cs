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

    public static event TypedEventHandler<object, bool>? LoadingStateChanged;

    private readonly SearchInput _searchInput;

    public SaveSearchForm()
    : this(SearchInput.SearchString)
    {
    }

    public SaveSearchForm(SearchInput input)
    {
        _searchInput = input;
    }

    public override string DataJson()
    {
        var dataName = _searchInput == SearchInput.SearchString ? "SaveSearchData" : "SaveSearchSurveyData";
        var path = Path.Combine(AppContext.BaseDirectory, GitHubHelper.GetTemplatePath(dataName));
        var data = File.ReadAllText(path, Encoding.Default) ?? throw new FileNotFoundException(path);
        return data;
    }

    public override string TemplateJson()
    {
        var templateName = _searchInput == SearchInput.SearchString ? "SaveSearch" : "SaveSearchSurvey";
        var path = Path.Combine(AppContext.BaseDirectory, GitHubHelper.GetTemplatePath(templateName));
        var template = File.ReadAllText(path, Encoding.Default) ?? throw new FileNotFoundException(path);

        return template;
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

    private void HandleSubmit(string payload)
    {
        var search = GetSearch(payload);
        ExtensionHost.LogMessage(new LogMessage() { Message = $"Search: {search}" });
    }

    private Search GetSearch(string payload)
    {
        try
        {
            var payloadJson = JsonNode.Parse(payload) ?? throw new InvalidOperationException("No search found");

            var search = _searchInput switch
            {
                SearchInput.SearchString => CreateSearchFromJson(payloadJson),
                SearchInput.Survey => CreateSearchFromSurveyJson(payloadJson),
                _ => throw new NotImplementedException(),
            };

            SearchSaved?.Invoke(this, search);
            return search;
        }
        catch (Exception ex)
        {
            SearchSaved?.Invoke(this, ex);
        }

        return new Search();
    }

    public static Search CreateSearchFromJson(JsonNode jsonNode)
    {
        var searchStr = jsonNode["EnteredSearch"]?.ToString() ?? string.Empty;

        var repoHelper = GitHubRepositoryHelper.Instance;
        repoHelper.ValidateSearch(searchStr).Wait();

        var search = new Search(searchStr);

        return search;
    }

    public static Search CreateSearchFromSurveyJson(JsonNode jsonNode)
    {
        var search = new Search(
            jsonNode["name"]?.ToString() ?? string.Empty,
            jsonNode["type"]?.ToString() ?? string.Empty,
            jsonNode["owner"]?.ToString() ?? string.Empty,
            jsonNode["repository"]?.ToString() ?? string.Empty,
            jsonNode["dateCreated"]?.ToString() ?? string.Empty,
            jsonNode["language"]?.ToString() ?? string.Empty,
            jsonNode["state"]?.ToString() ?? string.Empty,
            jsonNode["reason"]?.ToString() ?? string.Empty,
            jsonNode["numberOfComments"]?.ToString() ?? string.Empty,
            jsonNode["labels"]?.ToString() ?? string.Empty,
            jsonNode["author"]?.ToString() ?? string.Empty,
            jsonNode["mentionedUsers"]?.ToString() ?? string.Empty,
            jsonNode["assignees"]?.ToString() ?? string.Empty,
            jsonNode["updatedDate"]?.ToString() ?? string.Empty);

        return search;
    }
}
