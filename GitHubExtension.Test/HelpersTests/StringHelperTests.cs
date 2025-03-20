// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Helpers;

namespace GitHubExtension.Test.HelpersTests;

[TestClass]
public class StringHelperTests
{
    [TestMethod]
    public void SwapGitColonsForForwardSlashes_ReplacesColonsWithForwardSlashes()
    {
        var input = "refs/heads/feature:branch";
        var expected = "refs/heads/feature/branch";

        var result = StringHelper.SwapGitColonsForForwardSlashes(input);

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void SwapGitColonsForForwardSlashes_NoColons_ReturnsSameString()
    {
        var input = "refs/heads/feature-branch";
        var expected = "refs/heads/feature-branch";

        var result = StringHelper.SwapGitColonsForForwardSlashes(input);

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void SwapGitColonsForForwardSlashes_EmptyString_ReturnsEmptyString()
    {
        var input = string.Empty;
        var expected = string.Empty;

        var result = StringHelper.SwapGitColonsForForwardSlashes(input);

        Assert.AreEqual(expected, result);
    }
}
