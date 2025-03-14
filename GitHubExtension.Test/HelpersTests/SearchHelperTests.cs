// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataModel.Enums;
using GitHubExtension.Helpers;

namespace GitHubExtension.Test.HelpersTests;

[TestClass]
public class SearchHelperTests
{
    [TestMethod]
    public void ParseSearchTypeFromSearchString_ValidInput_ReturnsCorrectSearchTypeIssues()
    {
        var typeIssuesString = "type:issue";
        var expectedType = SearchType.Issues;

        var result = SearchHelper.ParseSearchTypeFromSearchString(typeIssuesString);

        Assert.AreEqual(expectedType, result);

        var isIssuesString = "is:issue";
        var resultIsIssues = SearchHelper.ParseSearchTypeFromSearchString(isIssuesString);
        Assert.AreEqual(expectedType, resultIsIssues);
    }

    [TestMethod]
    public void ParseSearchTypeFromSearchString_ValidInput_ReturnsCorrectSearchTypePullRequests()
    {
        var typePullRequestString = "type:pr";
        var expectedType = SearchType.PullRequests;

        var result = SearchHelper.ParseSearchTypeFromSearchString(typePullRequestString);

        Assert.AreEqual(expectedType, result);

        var isPullRequestString = "is:pr";

        var resultIsPR = SearchHelper.ParseSearchTypeFromSearchString(isPullRequestString);

        Assert.AreEqual(expectedType, resultIsPR);
    }

    [TestMethod]
    public void ParseSearchTypeFromSearchString_ValidInput_ReturnsCombinedTypeIfNoneProvided()
    {
        var combinedSearchString = "search";
        var expectedType = SearchType.IssuesAndPullRequests;

        var result = SearchHelper.ParseSearchTypeFromSearchString(combinedSearchString);

        Assert.AreEqual(expectedType, result);
    }

    [TestMethod]
    public void ParseSearchTypeFromSearchString_ValidInput_ReturnsCorrectSearchTypeRepositories()
    {
        var typeRepositoryString = "type:repo";
        var expectedType = SearchType.Repositories;

        var result = SearchHelper.ParseSearchTypeFromSearchString(typeRepositoryString);

        Assert.AreEqual(expectedType, result);

        var isRepositoryString = "is:repo";

        var resultIsRepo = SearchHelper.ParseSearchTypeFromSearchString(isRepositoryString);

        Assert.AreEqual(expectedType, resultIsRepo);
    }

    [TestMethod]
    public void ParseSearchTypeFromSearchString_ValidInput_HandlesIsState()
    {
        var isOpen = "is:open is:issue";
        var expectedType = SearchType.Issues;

        var result = SearchHelper.ParseSearchTypeFromSearchString(isOpen);

        Assert.AreEqual(expectedType, result);

        var isClosed = "is:closed is:issue";

        var resultIsClosed = SearchHelper.ParseSearchTypeFromSearchString(isClosed);

        Assert.AreEqual(expectedType, resultIsClosed);
    }
}
