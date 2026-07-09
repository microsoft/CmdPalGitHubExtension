// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;

namespace GitHubExtension.DataManager;

public interface IDiscussionsDataManager
{
    // Searches discussions using GitHub search syntax (GraphQL search with
    // type DISCUSSION). Pass "author:@me" for the user's own discussions.
    Task<IEnumerable<IDiscussion>> SearchDiscussionsAsync(string query);
}
