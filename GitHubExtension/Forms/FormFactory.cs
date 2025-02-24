// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Helpers;
using GitHubExtension.PersistentData;
using Microsoft.CommandPalette.Extensions;

namespace GitHubExtension.Forms;

public class FormFactory : IFormFactory
{
    private readonly ISearchHelper _searchHelper;

    public FormFactory(ISearchHelper searchHelper)
    {
        _searchHelper = searchHelper;
    }

    public IForm GetSaveSearchForm(SearchInput input)
    {
        var form = new SaveSearchForm(input, _searchHelper);
        return form;
    }

    public IForm GetSaveSearchForm(Search search)
    {
        var form = new SaveSearchForm(search, _searchHelper);
        return form;
    }
}
