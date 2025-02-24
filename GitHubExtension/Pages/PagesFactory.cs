// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Commands;
using GitHubExtension.Forms;
using GitHubExtension.Helpers;
using GitHubExtension.Pages;
using Microsoft.CommandPalette.Extensions;

namespace GitHubExtension;

public class PagesFactory : IPagesFactory
{
    private readonly ISearchHelper _searchHelper;
    private readonly IRepositoryHelper _repositoryHelper;
    private readonly IFormFactory _formFactory;
    private readonly SearchPageFactory _searchPageFactory;

    public PagesFactory(ISearchHelper searchHelper, IRepositoryHelper repositoryHelper, IFormFactory formFactory, SearchPageFactory searchPageFactory)
    {
        _repositoryHelper = repositoryHelper;
        _searchHelper = searchHelper;
        _formFactory = formFactory;
        _searchPageFactory = searchPageFactory;
    }

    public IPage GetSaveSearchPage()
    {
        return new SaveSearchPage(_formFactory);
    }

    public IPage GetSaveSearchPage(SearchInput input)
    {
        return new SaveSearchPage(input, _formFactory);
    }

    public IPage GetSavedSearchesPage()
    {
        return new SavedSearchesPage(_searchHelper, this);
    }

    public IPage GetEditSearchPage(PersistentData.Search search)
    {
        return new EditSearchPage(search, _formFactory);
    }

    public IInvokableCommand GetRemoveSavedSearchCommand(SearchCandidate search)
    {
        return new RemoveSavedSearchCommand(search, _searchHelper);
    }

    public IInvokableCommand GetRemoveSavedSearchCommand(PersistentData.Search search)
    {
        return new RemoveSavedSearchCommand(search, _searchHelper);
    }

    public IPage CreateForSearch(PersistentData.Search search)
    {
        return _searchPageFactory.CreateForSearch(search);
    }
}
