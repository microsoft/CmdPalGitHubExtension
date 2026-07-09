// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataManager.Data;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class GitHubCreateManagerTests
{
    [TestMethod]
    [TestCategory("Unit")]
    public void ParseOwnerRepo_ShortForm_ParsesOwnerAndRepo()
    {
        var (owner, repo) = GitHubCreateManager.ParseOwnerRepo("octocat/hello-world");

        Assert.AreEqual("octocat", owner);
        Assert.AreEqual("hello-world", repo);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ParseOwnerRepo_WithSurroundingWhitespace_Trims()
    {
        var (owner, repo) = GitHubCreateManager.ParseOwnerRepo("  octocat / hello-world  ");

        Assert.AreEqual("octocat", owner);
        Assert.AreEqual("hello-world", repo);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ParseOwnerRepo_FullUrl_ParsesOwnerAndRepo()
    {
        var (owner, repo) = GitHubCreateManager.ParseOwnerRepo("https://github.com/octocat/hello-world");

        Assert.AreEqual("octocat", owner);
        Assert.AreEqual("hello-world", repo);
    }

    [TestMethod]
    [TestCategory("Unit")]
    [ExpectedException(typeof(ArgumentException))]
    public void ParseOwnerRepo_Empty_Throws()
    {
        GitHubCreateManager.ParseOwnerRepo("   ");
    }

    [TestMethod]
    [TestCategory("Unit")]
    [ExpectedException(typeof(ArgumentException))]
    public void ParseOwnerRepo_MissingRepo_Throws()
    {
        GitHubCreateManager.ParseOwnerRepo("octocat");
    }
}
