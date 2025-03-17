// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;

namespace GitHubExtension.Helpers;

public static class GitHubRequestHelper
{
    public static SearchIssuesRequest GetSearchIssuesRequest(string term)
    {
        return new SearchIssuesRequest(term)
        {
            PerPage = 100,
            Page = 1,
        };
    }
}
