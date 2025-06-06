// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Helpers;

namespace GitHubExtension.Test.HelpersTests;

[TestClass]
public class StringHelperTests
{
    [DataRow("refs/heads/feature:branch", "refs/heads/feature/branch")]
    [DataRow("refs/heads/feature-branch", "refs/heads/feature-branch")]
    [DataRow("", "")]
    [TestMethod]
    public void SwapGitColonsForForwardSlashes_ReturnsExpectedResult(string input, string expected)
    {
        var result = StringHelper.SwapGitColonsForForwardSlashes(input);
        Assert.AreEqual(expected, result);
    }
}
