// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataManager;
using GitHubExtension.DataModel.DataObjects;
using Moq;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class PullRequestSourceBranchDecoratorTest
{
    [DataRow("owner:feature/branch", "https://github.com/owner/repo/pull/1", "feature/branch")]
    [DataRow("OWNER:feature/branch", "https://github.com/owner/repo/pull/1", "feature/branch")]
    [DataRow("feature/branch/update", "https://github.com/owner/repo/pull/1", "feature/branch/update")]
    [DataRow("", "https://github.com/owner/repo/pull/1", "")]
    [DataRow(null, "https://github.com/owner/repo/pull/1", "")]
    [DataRow("microsoft:main", "https://github.com/microsoft/devhome", "main")]
    [DataRow("microsoft:user/laurenciha/update", "https://github.com/microsoft/devhome", "user/laurenciha/update")]
    [TestMethod]
    public void RemoveOwnerFromSourceBranch_RemovesOwnerPrefix_WhenPresent(
        string sourceBranch, string htmlUrl, string expected)
    {
        var pr = new PullRequest
        {
            SourceBranch = sourceBranch,
            HtmlUrl = htmlUrl,
        };
        var updater = new Mock<IPullRequestUpdater>().Object;
        var decorator = new PullRequestSourceBranchDecorator(pr, updater);

        var result = decorator.RemoveOwnerFromSourceBranch(pr);

        Assert.AreEqual(expected, result);
    }
}
