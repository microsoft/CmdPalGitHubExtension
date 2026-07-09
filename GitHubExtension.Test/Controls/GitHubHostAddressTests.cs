// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Client;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class GitHubHostAddressTests
{
    [TestMethod]
    [TestCategory("Unit")]
    public void ParseHostInput_Empty_DefaultsToGitHubDotCom()
    {
        var uri = GitHubHostAddress.ParseHostInput(string.Empty);
        Assert.AreEqual("github.com", uri.Host);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ParseHostInput_BareHost_AssumesHttps()
    {
        var uri = GitHubHostAddress.ParseHostInput("github.example.com");
        Assert.AreEqual(Uri.UriSchemeHttps, uri.Scheme);
        Assert.AreEqual("github.example.com", uri.Host);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ParseHostInput_WithScheme_Preserved()
    {
        var uri = GitHubHostAddress.ParseHostInput("https://github.example.com");
        Assert.AreEqual("github.example.com", uri.Host);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ParseHostInput_UnsupportedScheme_Throws()
    {
        Assert.ThrowsException<UriFormatException>(() => GitHubHostAddress.ParseHostInput("ftp://github.example.com"));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void IsGitHubDotComHost_RecognizesCommonForms()
    {
        Assert.IsTrue(GitHubHostAddress.IsGitHubDotComHost("github.com"));
        Assert.IsTrue(GitHubHostAddress.IsGitHubDotComHost("www.github.com"));
        Assert.IsTrue(GitHubHostAddress.IsGitHubDotComHost("api.github.com"));
        Assert.IsFalse(GitHubHostAddress.IsGitHubDotComHost("github.example.com"));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GetApiBaseUri_GitHubDotCom_UsesApiGitHubCom()
    {
        var host = GitHubHostAddress.ParseHostInput("github.com");
        Assert.AreEqual("https://api.github.com/", GitHubHostAddress.GetApiBaseUri(host).ToString());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GetApiBaseUri_Enterprise_UsesApiV3Path()
    {
        var host = GitHubHostAddress.ParseHostInput("github.example.com");
        Assert.AreEqual("https://github.example.com/api/v3/", GitHubHostAddress.GetApiBaseUri(host).ToString());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GetGraphQLUri_GitHubDotCom_UsesApiGitHubComGraphQL()
    {
        var host = GitHubHostAddress.ParseHostInput("github.com");
        Assert.AreEqual("https://api.github.com/graphql", GitHubHostAddress.GetGraphQLUri(host).ToString());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GetGraphQLUri_Enterprise_UsesApiGraphQLPath()
    {
        var host = GitHubHostAddress.ParseHostInput("github.example.com");
        Assert.AreEqual("https://github.example.com/api/graphql", GitHubHostAddress.GetGraphQLUri(host).ToString());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GetGraphQLUriFromApiBase_GitHubDotCom()
    {
        var apiBase = new Uri("https://api.github.com/");
        Assert.AreEqual("https://api.github.com/graphql", GitHubHostAddress.GetGraphQLUriFromApiBase(apiBase).ToString());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GetGraphQLUriFromApiBase_Enterprise()
    {
        var apiBase = new Uri("https://github.example.com/api/v3/");
        Assert.AreEqual("https://github.example.com/api/graphql", GitHubHostAddress.GetGraphQLUriFromApiBase(apiBase).ToString());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GraphQLClient_ResolveGraphQLEndpoint_DelegatesToHostAddress()
    {
        Assert.AreEqual(
            "https://api.github.com/graphql",
            GitHubGraphQLClient.ResolveGraphQLEndpoint(new Uri("https://api.github.com/")).ToString());
        Assert.AreEqual(
            "https://github.example.com/api/graphql",
            GitHubGraphQLClient.ResolveGraphQLEndpoint(new Uri("https://github.example.com/api/v3/")).ToString());
    }
}
