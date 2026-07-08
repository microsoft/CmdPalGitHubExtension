// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataModel.Enums;
using GitHubExtension.Helpers;
using Octokit;

namespace GitHubExtension.Test.HelpersTests;

[TestClass]
public class SearchHelperTests
{
    [DataRow("type:issue", SearchType.Issues)]
    [DataRow("is:issue", SearchType.Issues)]
    [DataRow("type:pr", SearchType.PullRequests)]
    [DataRow("is:pr", SearchType.PullRequests)]
    [DataRow("type:repo", SearchType.Repositories)]
    [DataRow("is:repo", SearchType.Repositories)]
    [DataRow("search", SearchType.IssuesAndPullRequests)]
    [DataRow("is:open is:issue", SearchType.Issues)]
    [DataRow("is:closed is:issue", SearchType.Issues)]
    [TestMethod]
    public void ParseSearchTypeFromSearchString_ValidInput_ReturnsExpectedType(string input, SearchType expectedType)
    {
        var result = SearchHelper.ParseSearchTypeFromSearchString(input);
        Assert.AreEqual(expectedType, result);
    }

    [DataRow("https://github.com/search?q=repo:microsoft/PowerToys+is:issue+label:bug", "repo:microsoft/PowerToys is:issue label:bug")]
    [DataRow("https://github.com/microsoft/PowerToys/issues?q=is:open+label:bug", "repo:microsoft/PowerToys is:open label:bug")]
    [DataRow("https://github.com/microsoft/PowerToys/issues", "repo:microsoft/PowerToys is:issue is:open")]
    [DataRow("https://github.com/microsoft/PowerToys/issues?q=is:issue+is:closed", "repo:microsoft/PowerToys is:issue is:closed")]
    [DataRow("https://github.com/microsoft/PowerToys/pulls", "repo:microsoft/PowerToys is:pr is:open")]
    [DataRow("https://github.com/search/issues", "is:issue")]
    [DataRow("https://github.contoso.com/search?q=repo:internal/project+is:issue", "repo:internal/project is:issue")]
    [DataRow("https://github.com/search?q=repo:microsoft/PowerToys+is:open+is:issue+label:bug+author:octocat", "repo:microsoft/PowerToys is:open is:issue label:bug author:octocat")]
    [DataRow("https://github.com/search?q=repo:microsoft/PowerToys+is:open+-label:wontfix", "repo:microsoft/PowerToys is:open -label:wontfix")]
    [DataRow("https://github.com/microsoft/PowerToys/issues?q=is%3Aopen%20is%3Aissue%20label%3A%22Product-Command%20Palette%22%20-label%3ARun-Plugin", "repo:microsoft/PowerToys is:open is:issue label:\"Product-Command Palette\" -label:Run-Plugin")]
    [DataRow("https://github.com/microsoft/PowerToys/issues?q=is%3Aopen%20is%3Aissue%20label%3A%22Good%20first%20issue%22", "repo:microsoft/PowerToys is:open is:issue label:\"Good first issue\"")]
    [DataRow("https://github.com/search?q=repo:microsoft/terminal+repo:microsoft/PowerToys+repo:microsoft/vscode+is:open+is:issue", "repo:microsoft/terminal repo:microsoft/PowerToys repo:microsoft/vscode is:open is:issue")]
    [TestMethod]
    public void ParseSearchStringFromUri_ValidInput_ReturnsExpectedSearchString(string uriString, string expected)
    {
        var uri = new Uri(uriString);
        var result = SearchHelper.ParseSearchStringFromUri(uri);
        Assert.AreEqual(expected, result);
    }

    [DataRow("user:octocat type:repository", "user:octocat")]
    [DataRow("type:repo user:octocat", "user:octocat")]
    [DataRow("user:octocat", "user:octocat")]
    [DataRow("", "")]
    [TestMethod]
    public void RemoveTypeQualifier_StripsTypeToken(string input, string expected)
    {
        var result = SearchHelper.RemoveTypeQualifier(input);
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void ParseRepoSortFromTerm_ParsesFieldDirectionAndStripsToken()
    {
        var result = SearchHelper.ParseRepoSortFromTerm("user:octocat sort:updated-desc type:repository");
        Assert.IsNotNull(result);
        Assert.AreEqual(RepoSearchSort.Updated, result.Value.SortField);
        Assert.AreEqual(SortDirection.Descending, result.Value.Direction);
        Assert.AreEqual("user:octocat type:repository", result.Value.UpdatedTerm);
    }

    [DataRow("stars", RepoSearchSort.Stars)]
    [DataRow("forks", RepoSearchSort.Forks)]
    [DataRow("updated", RepoSearchSort.Updated)]
    [TestMethod]
    public void ParseRepoSortFromTerm_MapsKnownFields(string field, RepoSearchSort expected)
    {
        var result = SearchHelper.ParseRepoSortFromTerm($"sort:{field}-asc");
        Assert.IsNotNull(result);
        Assert.AreEqual(expected, result.Value.SortField);
        Assert.AreEqual(SortDirection.Ascending, result.Value.Direction);
    }

    [TestMethod]
    public void ParseRepoSortFromTerm_NoSort_ReturnsNull()
    {
        Assert.IsNull(SearchHelper.ParseRepoSortFromTerm("user:octocat"));
    }

    [TestMethod]
    public void GetSearchRepositoriesRequest_AppliesSortFromTerm()
    {
        var request = GitHubRequestHelper.GetSearchRepositoriesRequest("user:octocat sort:updated-desc type:repository");
        Assert.AreEqual(RepoSearchSort.Updated, request.SortField);
        Assert.AreEqual(SortDirection.Descending, request.Order);
        Assert.AreEqual(ExtensionConstants.PerPage, request.PerPage);
    }
}
