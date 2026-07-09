// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using GitHubExtension.Controls;
using GitHubExtension.Controls.Forms;
using GitHubExtension.DataManager;
using GitHubExtension.Helpers;
using Moq;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class CreateFormsTests
{
    private static Mock<IResources> CreateResources()
    {
        var resources = new Mock<IResources>();
        resources.Setup(x => x.GetResource(It.IsAny<string>(), null)).Returns<string, object>((key, _) => key);
        return resources;
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void CreateIssueForm_TemplateJson_IsValidJson()
    {
        var createManager = new Mock<IGitHubCreateManager>();
        var form = new CreateIssueForm(createManager.Object, CreateResources().Object, "owner/repo");

        var json = form.TemplateJson;

        var parsed = JsonDocument.Parse(json);
        Assert.AreEqual("AdaptiveCard", parsed.RootElement.GetProperty("type").GetString());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void CreatePullRequestForm_TemplateJson_IsValidJson()
    {
        var createManager = new Mock<IGitHubCreateManager>();
        var form = new CreatePullRequestForm(createManager.Object, CreateResources().Object);

        var json = form.TemplateJson;

        var parsed = JsonDocument.Parse(json);
        Assert.AreEqual("AdaptiveCard", parsed.RootElement.GetProperty("type").GetString());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void CreateBranchForm_TemplateJson_IsValidJson()
    {
        var createManager = new Mock<IGitHubCreateManager>();
        var form = new CreateBranchForm(createManager.Object, CreateResources().Object);

        var json = form.TemplateJson;

        var parsed = JsonDocument.Parse(json);
        Assert.AreEqual("AdaptiveCard", parsed.RootElement.GetProperty("type").GetString());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void AddCommentForm_TemplateJson_IsValidJson()
    {
        var createManager = new Mock<IGitHubCreateManager>();
        var issue = new Mock<IIssue>();
        issue.Setup(x => x.Title).Returns("An issue title");
        var form = new AddCommentForm(issue.Object, createManager.Object, CreateResources().Object);

        var json = form.TemplateJson;

        var parsed = JsonDocument.Parse(json);
        Assert.AreEqual("AdaptiveCard", parsed.RootElement.GetProperty("type").GetString());
    }
}
