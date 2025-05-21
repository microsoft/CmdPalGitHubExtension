// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using System.Text.Json.Nodes;
using GitHubExtension.Client;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Serilog;

namespace GitHubExtension.Controls.Forms;

public sealed partial class SaveSearchForm : FormContent, IGitHubForm
{
    private readonly ISearch _savedSearch;

    private readonly ISearchRepository _searchRepository;

    private readonly IResources _resources;

    private readonly SavedSearchesMediator _savedSearchesMediator;

    private string IsTopLevelChecked => GetIsTopLevel().Result.ToString().ToLower(CultureInfo.InvariantCulture);

    public event EventHandler<bool>? LoadingStateChanged;

    public event EventHandler<FormSubmitEventArgs>? FormSubmitted;

    public Dictionary<string, string> TemplateSubstitutions => new()
    {
        { "{{SaveSearchFormTitle}}", _resources.GetResource(string.IsNullOrWhiteSpace(_savedSearch.Name) ? "Forms_Save_Search" : "Forms_Edit_Search") },
        { "{{SavedSearchString}}", _savedSearch.SearchString },
        { "{{SavedSearchName}}", _savedSearch.Name },
        { "{{IsTopLevel}}", IsTopLevelChecked },
        { "{{EnteredSearchErrorMessage}}", _resources.GetResource("Forms_SaveSearchTemplateEnteredSearchError") },
        { "{{EnteredSearchLabel}}", _resources.GetResource("Forms_SaveSearchTemplateEnteredSearchLabel") },
        { "{{NameLabel}}", _resources.GetResource("Forms_SaveSearchTemplateNameLabel") },
        { "{{NameErrorMessage}}", _resources.GetResource("Forms_SaveSearchTemplateNameError") },
        { "{{IsTopLevelTitle}}", _resources.GetResource("Forms_SaveSearchTemplateIsTopLevelTitle") },
        { "{{SaveSearchActionTitle}}", _resources.GetResource(string.IsNullOrWhiteSpace(_savedSearch.Name) ? "Forms_SaveSearchTemplateSaveSearchActionTitle" : "Forms_SaveSearchTemplateEditSearchActionTitle") },
    };

    // for saving a new query
    public SaveSearchForm(ISearchRepository searchRepository, IResources resources, SavedSearchesMediator savedSearchesMediator)
    {
        _resources = resources;
        _savedSearch = new SearchCandidate();
        _searchRepository = searchRepository;
        _savedSearchesMediator = savedSearchesMediator;
    }

    // for editing an existing query
    public SaveSearchForm(ISearch savedSearch, ISearchRepository searchRepository, IResources resources, SavedSearchesMediator savedSearchesMediator)
    {
        _resources = resources;
        _savedSearch = savedSearch;
        _searchRepository = searchRepository;
        _savedSearchesMediator = savedSearchesMediator;
    }

    public override string TemplateJson => TemplateHelper.LoadTemplateJsonFromTemplateName("SaveSearch", TemplateSubstitutions);

    public override ICommandResult SubmitForm(string? inputs, string data)
    {
        LoadingStateChanged?.Invoke(this, true);
        Task.Run(async () =>
        {
            var search = await GetSearchAsync(inputs);
            ExtensionHost.LogMessage(new LogMessage() { Message = $"Search: {search}" });
        });

        return CommandResult.KeepOpen();
    }

    public async Task<SearchCandidate> GetSearchAsync(string? payload)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                return new SearchCandidate();
            }

            var payloadJson = JsonNode.Parse(payload) ?? throw new InvalidOperationException("No search found");

            var search = CreateSearchFromJson(payloadJson);

            // if editing the search, delete the old one
            // it is safe to do as the new one is already validated
            if (_savedSearch.SearchString != string.Empty)
            {
                Log.Information($"Removing outdated search {_savedSearch.Name}, {_savedSearch.SearchString}");

                // Remove deleted search from top-level commands
                await _searchRepository.UpdateSearchTopLevelStatus(_savedSearch, false);
                await _searchRepository.RemoveSavedSearch(_savedSearch);
            }

            // UpdateSearchTopLevelStatus adds the search if it's not already in the datastore
            await _searchRepository.UpdateSearchTopLevelStatus(search, search.IsTopLevel);

            LoadingStateChanged?.Invoke(this, false);
            _savedSearchesMediator.AddSearch(new SavedSearchesUpdatedEventArgs(true, null, search));
            FormSubmitted?.Invoke(this, new FormSubmitEventArgs(true, null));
            return search;
        }
        catch (Exception ex)
        {
            _savedSearchesMediator.AddSearch(new SavedSearchesUpdatedEventArgs(false, ex, null));
            FormSubmitted?.Invoke(this, new FormSubmitEventArgs(false, ex));
        }

        return new SearchCandidate();
    }

    public static SearchCandidate CreateSearchFromJson(JsonNode? jsonNode)
    {
        var enteredSearch = jsonNode?["EnteredSearch"]?.ToString() ?? string.Empty;
        var name = jsonNode?["Name"]?.ToString() ?? string.Empty;
        var isTopLevel = jsonNode?["IsTopLevel"]?.ToString() == "true";

        string? searchStr;
        var searchFromUrl = string.Empty;
        if (Validation.IsValidHttpUri(enteredSearch, out Uri? uri) && uri != null)
        {
            searchFromUrl = SearchHelper.ParseSearchStringFromUri(uri);
        }

        searchStr = string.IsNullOrWhiteSpace(searchFromUrl) ? enteredSearch : searchFromUrl;
        name = string.IsNullOrWhiteSpace(name) ? searchStr : name;

        return new SearchCandidate(searchStr, name, isTopLevel);
    }

    public async Task<bool> GetIsTopLevel()
    {
        return await _searchRepository.IsTopLevel(_savedSearch);
    }
}
