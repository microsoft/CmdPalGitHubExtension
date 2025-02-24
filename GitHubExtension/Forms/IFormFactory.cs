// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;

namespace GitHubExtension.Forms;

public interface IFormFactory
{
    public IForm GetSaveSearchForm(SearchInput input);

    public IForm GetSaveSearchForm(PersistentData.Search search);
}
