// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataModel.Enums;
using GitHubExtension.Helpers;

namespace GitHubExtension.Test.HelpersTests;

[TestClass]
public class SearchHelperTests
{
    // ParseSearchStringFromUri doesn't accept null input, so that case is not tested.
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

    [TestMethod]
    public void ParseSearchStringFromUri_WithQueryParameter_ReturnsSearchString()
    {
        var uri = new Uri("https://github.com/search?q=repo:microsoft/PowerToys+is:issue+label:bug");
        var expected = "repo:microsoft/PowerToys is:issue label:bug";

        var result = SearchHelper.ParseSearchStringFromUri(uri);

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void ParseSearchStringFromUri_WithRepositoryIssuesUrl_ReturnsFormattedSearchString()
    {
        var uri = new Uri("https://github.com/microsoft/PowerToys/issues?q=is:open+label:bug");
        var expected = "repo:microsoft/PowerToys is:open label:bug";

        var result = SearchHelper.ParseSearchStringFromUri(uri);

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void ParseSearchStringFromUri_WithRepositoryIssuesWithoutQuery_ReturnsRepoBasedSearchString()
    {
        var uri = new Uri("https://github.com/microsoft/PowerToys/issues");
        var expected = "repo:microsoft/PowerToys is:issue is:open";

        var result = SearchHelper.ParseSearchStringFromUri(uri);

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void ParseSearchStringFromUri_WithRepositoryClosedIssuesUrl_ReturnsClosedIssuesSearchString()
    {
        var uri = new Uri("https://github.com/microsoft/PowerToys/issues?q=is:issue+is:closed");
        var expected = "repo:microsoft/PowerToys is:issue is:closed";

        var result = SearchHelper.ParseSearchStringFromUri(uri);

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void ParseSearchStringFromUri_WithPullRequestsUrl_ReturnsPrSearchString()
    {
        var uri = new Uri("https://github.com/microsoft/PowerToys/pulls");
        var expected = "repo:microsoft/PowerToys is:pr is:open";

        var result = SearchHelper.ParseSearchStringFromUri(uri);

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void ParseSearchStringFromUri_WithSearchPagesUrl_ReturnsBasicSearchString()
    {
        var uri = new Uri("https://github.com/search/issues");
        var expected = "is:issue";

        var result = SearchHelper.ParseSearchStringFromUri(uri);

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void ParseSearchStringFromUri_WithEnterpriseGitHubUrl_ReturnsSearchString()
    {
        var uri = new Uri("https://github.contoso.com/search?q=repo:internal/project+is:issue");
        var expected = "repo:internal/project is:issue";

        var result = SearchHelper.ParseSearchStringFromUri(uri);

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void ParseSearchStringFromUri_WithMultipleQualifiers_PreservesAllQualifiers()
    {
        var uri = new Uri("https://github.com/search?q=repo:microsoft/PowerToys+is:open+is:issue+label:bug+author:octocat");
        var expected = "repo:microsoft/PowerToys is:open is:issue label:bug author:octocat";

        var result = SearchHelper.ParseSearchStringFromUri(uri);

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void ParseSearchStringFromUri_WithNegatedQualifiers_PreservesNegation()
    {
        var uri = new Uri("https://github.com/search?q=repo:microsoft/PowerToys+is:open+-label:wontfix");
        var expected = "repo:microsoft/PowerToys is:open -label:wontfix";

        var result = SearchHelper.ParseSearchStringFromUri(uri);

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void ParseSearchStringFromUri_WithPositiveAndNegativeLabels_ReturnsCorrectSearchString()
    {
        var uri = new Uri("https://github.com/microsoft/PowerToys/issues?q=is%3Aopen%20is%3Aissue%20label%3A%22Product-Command%20Palette%22%20-label%3ARun-Plugin");
        var expected = "repo:microsoft/PowerToys is:open is:issue label:\"Product-Command Palette\" -label:Run-Plugin";

        var result = SearchHelper.ParseSearchStringFromUri(uri);

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void ParseSearchStringFromUri_WithPositiveLabel_ReturnsCorrectSearchString()
    {
        var uri = new Uri("https://github.com/microsoft/PowerToys/issues?q=is%3Aopen%20is%3Aissue%20label%3A%22Good%20first%20issue%22");
        var expected = "repo:microsoft/PowerToys is:open is:issue label:\"Good first issue\"";

        var result = SearchHelper.ParseSearchStringFromUri(uri);

        Assert.AreEqual(expected, result);
    }
}
