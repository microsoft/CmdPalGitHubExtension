// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.DataModel.DataObjects;

namespace GitHubExtension.DataManager;

public interface IDataRequester
{
    Search? GetSearch(string name, string searchString);

    IEnumerable<Issue> GetIssuesForSearch(string name, string searchString);

    IEnumerable<PullRequest> GetPullRequestsForSearch(string name, string searchString);
}
