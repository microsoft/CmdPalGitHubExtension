﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.Helpers;

internal interface ISearchHelper
{
    public Task<IEnumerable<PersistentData.Search>> GetSavedSearches();
}
