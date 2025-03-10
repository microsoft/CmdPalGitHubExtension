// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;

namespace GitHubExtension.Controls.Pages;

public interface ISearchPageFactory
{
    IListItem CreateItemForSearch(ISearch search);
}
