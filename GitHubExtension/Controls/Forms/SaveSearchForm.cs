// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;
using System.Text.Json.Nodes;
using GitHubExtension.Helpers;
using GitHubExtension.Pages;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Serilog;
using Windows.Foundation;

namespace GitHubExtension.Forms;

public sealed partial class SaveSearchForm : GitHubForm
{
    public static event TypedEventHandler<object, object?>? SearchSaved;

    private readonly SearchInput _searchInput;

    private readonly ISearch _savedSearch;

    private readonly ISearchRepository _searchRepository;

    public override ICommandResult DefaultSubmitFormCommand => CommandResult.KeepOpen();

    public override Dictionary<string, string> TemplateSubstitutions => new()
    {
        { "{{SaveSearchFormTitle}}", string.IsNullOrEmpty(_savedSearch.Name) ? "Save Search" : "Edit Search" },
        { "{{SavedSearchString}}", _savedSearch.SearchString },
        { "{{SavedSearchName}}", _savedSearch.Name },
    };

    public SaveSearchForm(SearchInput input, ISearchRepository searchRepository)
    {
        _searchInput = input;
        _savedSearch = new SearchCandidate();
        _searchRepository = searchRepository;
    }

    public SaveSearchForm(ISearch savedSearch, ISearchRepository searchRepository)
    {
        _searchInput = SearchInput.SearchString;
        _savedSearch = savedSearch;
        _searchRepository = searchRepository;
    }

    public override string DataJson => LoadDataJson(_searchInput);

    private string LoadDataJson(SearchInput searchInput)
    {
        if (searchInput == SearchInput.SearchString)
        {
            return string.Empty;
        }

        var path = Path.Combine(AppContext.BaseDirectory, GitHubHelper.GetTemplatePath("SaveSearchSurveyData"));
        var data = File.ReadAllText(path, Encoding.Default) ?? throw new FileNotFoundException(path);
        return data;
    }

    public override string TemplateJson => LoadTemplateJsonFromFile(_searchInput == SearchInput.SearchString ? "SaveSearch" : "SaveSearchSurvey");

    public override void HandleSubmit(string payload)
    {
        var search = GetSearchAsync(payload);
        ExtensionHost.LogMessage(new LogMessage() { Message = $"Search: {search}" });
    }

    private async Task<SearchCandidate> GetSearchAsync(string payload)
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

            await _searchRepository.ValidateSearch(search);
            await _searchRepository.AddSavedSearch(search);

            // if editing the search, delete the old one
            if (_savedSearch.SearchString != string.Empty)
            {
                Log.Information($"Removing outdated search {_savedSearch.Name}, {_savedSearch.SearchString}");
                _searchRepository.RemoveSavedSearch(_savedSearch).Wait();
            }

            RaiseLoadingStateChanged(false);
            SearchSaved?.Invoke(this, search);
            RaiseFormSubmitted(new FormSubmitEventArgs(true, null));
            return search;
        }
        catch (Exception ex)
        {
            RaiseLoadingStateChanged(false);
            SearchSaved?.Invoke(this, ex);
            RaiseFormSubmitted(new FormSubmitEventArgs(false, ex));
        }

        return new SearchCandidate();
    }

    public static SearchCandidate CreateSearchFromJson(JsonNode jsonNode)
    {
        var searchStr = jsonNode["EnteredSearch"]?.ToString() ?? string.Empty;
        var name = jsonNode["Name"]?.ToString() ?? string.Empty;

        var search = new SearchCandidate(searchStr, name);

        return search;
    }

    public static SearchCandidate CreateSearchFromSurveyJson(JsonNode jsonNode)
    {
        var search = new SearchCandidate(
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
