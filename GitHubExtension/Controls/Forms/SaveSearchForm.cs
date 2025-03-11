// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Nodes;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Serilog;
using Windows.Foundation;

namespace GitHubExtension.Controls.Forms;

public sealed partial class SaveSearchForm : FormContent, IGitHubForm
{
    public static event TypedEventHandler<object, object?>? SearchSaved;

    private readonly ISearch _savedSearch;

    private readonly ISearchRepository _searchRepository;

    // IFormWithEvents implementation
    public event TypedEventHandler<object, bool>? LoadingStateChanged;

    public event TypedEventHandler<object, FormSubmitEventArgs>? FormSubmitted;

    public Dictionary<string, string> TemplateSubstitutions => new()
            {
                { "{{SaveSearchFormTitle}}", string.IsNullOrEmpty(_savedSearch.Name) ? "Save Search" : "Edit Search" },
                { "{{SavedSearchString}}", _savedSearch.SearchString },
                { "{{SavedSearchName}}", _savedSearch.Name },
            };

    public SaveSearchForm(ISearchRepository searchRepository)
    {
        _savedSearch = new SearchCandidate();
        _searchRepository = searchRepository;
    }

    public SaveSearchForm(ISearch savedSearch, ISearchRepository searchRepository)
    {
        _savedSearch = savedSearch;
        _searchRepository = searchRepository;
    }

    public override string TemplateJson => TemplateHelper.LoadTemplateJsonFromTemplateName("SaveSearch", TemplateSubstitutions);

    public override ICommandResult SubmitForm(string? inputs, string data)
    {
        LoadingStateChanged?.Invoke(this, true);
        Task.Run(() =>
        {
            var search = GetSearchAsync(inputs);
            ExtensionHost.LogMessage(new LogMessage() { Message = $"Search: {search}" });
        });

        return CommandResult.KeepOpen();
    }

    private async Task<SearchCandidate> GetSearchAsync(string? payload)
    {
        try
        {
            if (string.IsNullOrEmpty(payload))
            {
                return new SearchCandidate();
            }

            var payloadJson = JsonNode.Parse(payload) ?? throw new InvalidOperationException("No search found");

            var search = CreateSearchFromJson(payloadJson);

            await _searchRepository.ValidateSearch(search);
            await _searchRepository.AddSavedSearch(search);

            // if editing the search, delete the old one
            if (_savedSearch.SearchString != string.Empty)
            {
                Log.Information($"Removing outdated search {_savedSearch.Name}, {_savedSearch.SearchString}");
                _searchRepository.RemoveSavedSearch(_savedSearch).Wait();
            }

            LoadingStateChanged?.Invoke(this, false);
            SearchSaved?.Invoke(this, search);
            FormSubmitted?.Invoke(this, new FormSubmitEventArgs(true, null));
            return search;
        }
        catch (Exception ex)
        {
            LoadingStateChanged?.Invoke(this, false);
            SearchSaved?.Invoke(this, ex);
            FormSubmitted?.Invoke(this, new FormSubmitEventArgs(false, ex));
        }

        return new SearchCandidate();
    }

    public static SearchCandidate CreateSearchFromJson(JsonNode? jsonNode)
    {
        var searchStr = jsonNode?["EnteredSearch"]?.ToString() ?? string.Empty;
        var name = jsonNode?["Name"]?.ToString() ?? string.Empty;
        var isTopLevel = jsonNode?["IsTopLevel"]?.ToString() == "true";

        var search = new SearchCandidate(searchStr, name, isTopLevel);

        return search;
    }
}
