// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Helpers;

namespace GitHubExtension.Test.HelpersTests;

[TestClass]
public class NotificationHelperTests
{
    [TestMethod]
    [TestCategory("Unit")]
    [DataRow("https://api.github.com/repos/octocat/hello/issues/42", "Issue", "https://github.com/octocat/hello", "https://github.com/octocat/hello/issues/42")]
    [DataRow("https://api.github.com/repos/octocat/hello/pulls/7", "PullRequest", "https://github.com/octocat/hello", "https://github.com/octocat/hello/pull/7")]
    [DataRow("https://api.github.com/repos/octocat/hello/commits/abc123", "Commit", "https://github.com/octocat/hello", "https://github.com/octocat/hello/commit/abc123")]
    public void GetHtmlUrl_ConvertsApiUrlToBrowserUrl(string apiUrl, string type, string repoHtmlUrl, string expected)
    {
        var result = NotificationHelper.GetHtmlUrl(apiUrl, type, repoHtmlUrl);
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GetHtmlUrl_ReleaseFallsBackToRepositoryReleases()
    {
        var result = NotificationHelper.GetHtmlUrl("https://api.github.com/repos/octocat/hello/releases/99", "Release", "https://github.com/octocat/hello");
        Assert.AreEqual("https://github.com/octocat/hello/releases", result);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GetHtmlUrl_NullSubjectFallsBackToRepositoryUrl()
    {
        var result = NotificationHelper.GetHtmlUrl(null, "Discussion", "https://github.com/octocat/hello");
        Assert.AreEqual("https://github.com/octocat/hello", result);
    }
}
