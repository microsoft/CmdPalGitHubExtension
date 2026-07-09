// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using GitHubExtension.Controls;
using GitHubExtension.Controls.Forms;
using GitHubExtension.Controls.Pages;
using GitHubExtension.DataManager;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Moq;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class WorkflowRunsTests
{
    private static Mock<IResources> CreateResources()
    {
        var resources = new Mock<IResources>();
        resources.Setup(x => x.GetResource(It.IsAny<string>(), null)).Returns<string, object>((key, _) => key);
        return resources;
    }

    private static Mock<IWorkflowRun> CreateRun(long id, string status, string conclusion)
    {
        var run = new Mock<IWorkflowRun>();
        run.Setup(x => x.Id).Returns(id);
        run.Setup(x => x.Name).Returns("CI");
        run.Setup(x => x.RunNumber).Returns(id);
        run.Setup(x => x.Status).Returns(status);
        run.Setup(x => x.Conclusion).Returns(conclusion);
        run.Setup(x => x.HeadBranch).Returns("main");
        run.Setup(x => x.HtmlUrl).Returns($"https://github.com/owner/repo/actions/runs/{id}");
        return run;
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void TriggerWorkflowForm_TemplateJson_IsValidJson()
    {
        var dataManager = new Mock<IWorkflowRunsDataManager>();
        var form = new TriggerWorkflowForm(dataManager.Object, CreateResources().Object, "owner/repo");

        var json = form.TemplateJson;

        var parsed = JsonDocument.Parse(json);
        Assert.AreEqual("AdaptiveCard", parsed.RootElement.GetProperty("type").GetString());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void WorkflowRunsPage_EmptyQuery_ShowsPrompt()
    {
        var dataManager = new Mock<IWorkflowRunsDataManager>();
        var page = new WorkflowRunsPage(dataManager.Object, new WorkflowRunsMediator(), CreateResources().Object);

        var items = page.GetItems();

        Assert.AreEqual(1, items.Length);
        Assert.AreEqual("Pages_WorkflowRuns_Prompt", items[0].Title);
        dataManager.Verify(x => x.GetWorkflowRunsAsync(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void WorkflowRunsPage_WithRuns_ReturnsItems()
    {
        var dataManager = new Mock<IWorkflowRunsDataManager>();
        dataManager.Setup(x => x.GetWorkflowRunsAsync("owner/repo"))
            .ReturnsAsync(new[] { CreateRun(1, "completed", "success").Object, CreateRun(2, "completed", "failure").Object });

        var page = new WorkflowRunsPage(dataManager.Object, new WorkflowRunsMediator(), CreateResources().Object)
        {
            SearchText = "owner/repo",
        };

        var items = page.GetItems();

        Assert.AreEqual(2, items.Length);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void WorkflowRunsPage_InProgressRun_HasCancelCommand()
    {
        var dataManager = new Mock<IWorkflowRunsDataManager>();
        dataManager.Setup(x => x.GetWorkflowRunsAsync("owner/repo"))
            .ReturnsAsync(new[] { CreateRun(1, "in_progress", string.Empty).Object });

        var page = new WorkflowRunsPage(dataManager.Object, new WorkflowRunsMediator(), CreateResources().Object)
        {
            SearchText = "owner/repo",
        };

        var item = (ListItem)page.GetItems()[0];

        // Re-run + Cancel + Copy URL
        Assert.AreEqual(3, item.MoreCommands.Length);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void WorkflowRunsPage_CompletedRun_HasNoCancelCommand()
    {
        var dataManager = new Mock<IWorkflowRunsDataManager>();
        dataManager.Setup(x => x.GetWorkflowRunsAsync("owner/repo"))
            .ReturnsAsync(new[] { CreateRun(1, "completed", "success").Object });

        var page = new WorkflowRunsPage(dataManager.Object, new WorkflowRunsMediator(), CreateResources().Object)
        {
            SearchText = "owner/repo",
        };

        var item = (ListItem)page.GetItems()[0];

        // Re-run + Copy URL (no cancel for a completed run)
        Assert.AreEqual(2, item.MoreCommands.Length);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void WorkflowRunsPage_NoRuns_ShowsNoneMessage()
    {
        var dataManager = new Mock<IWorkflowRunsDataManager>();
        dataManager.Setup(x => x.GetWorkflowRunsAsync("owner/repo"))
            .ReturnsAsync(Array.Empty<IWorkflowRun>());

        var page = new WorkflowRunsPage(dataManager.Object, new WorkflowRunsMediator(), CreateResources().Object)
        {
            SearchText = "owner/repo",
        };

        var items = page.GetItems();

        Assert.AreEqual(1, items.Length);
        Assert.AreEqual("Pages_WorkflowRuns_None", items[0].Title);
    }
}
