// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.Controls.Commands;
using GitHubExtension.DataManager;
using GitHubExtension.Helpers;
using Moq;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class MutationCommandsFactoryTests
{
    private static (MutationCommandsFactory Factory, Mock<IGitHubMutationManager> Manager, MutationMediator Mediator) CreateFactory()
    {
        var manager = new Mock<IGitHubMutationManager>();
        var createManager = new Mock<IGitHubCreateManager>();
        var mediator = new MutationMediator();
        var resources = new Mock<IResources>();
        resources.Setup(x => x.GetResource(It.IsAny<string>(), null)).Returns<string, object>((key, _) => key);
        var factory = new MutationCommandsFactory(manager.Object, createManager.Object, mediator, resources.Object);
        return (factory, manager, mediator);
    }

    private static Mock<IIssue> CreateIssue(string state)
    {
        var issue = new Mock<IIssue>();
        issue.Setup(x => x.State).Returns(state);
        issue.Setup(x => x.HtmlUrl).Returns("https://github.com/owner/repo/issues/1");
        issue.Setup(x => x.Number).Returns(1);
        issue.Setup(x => x.Title).Returns("Title");
        return issue;
    }

    private static Mock<IPullRequest> CreatePullRequest(string state)
    {
        var pr = new Mock<IPullRequest>();
        pr.Setup(x => x.State).Returns(state);
        pr.Setup(x => x.HtmlUrl).Returns("https://github.com/owner/repo/pull/2");
        pr.Setup(x => x.Number).Returns(2);
        pr.Setup(x => x.Title).Returns("Title");
        pr.Setup(x => x.SourceBranch).Returns("feature");
        return pr;
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GetIssueCommands_OpenIssue_ReturnsCloseCommand()
    {
        var (factory, _, _) = CreateFactory();
        var commands = factory.GetIssueCommands(CreateIssue("Open").Object).ToList();

        Assert.AreEqual(2, commands.Count);
        Assert.AreEqual("Commands_CloseIssue", commands[0].Command!.Name);
        Assert.IsTrue(commands[0].IsCritical);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GetIssueCommands_ClosedIssue_ReturnsReopenCommand()
    {
        var (factory, _, _) = CreateFactory();
        var commands = factory.GetIssueCommands(CreateIssue("Closed").Object).ToList();

        Assert.AreEqual(2, commands.Count);
        Assert.AreEqual("Commands_ReopenIssue", commands[0].Command!.Name);
        Assert.IsFalse(commands[0].IsCritical);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GetPullRequestCommands_OpenPullRequest_ReturnsMergeAndCloseCommands()
    {
        var (factory, _, _) = CreateFactory();
        var commands = factory.GetPullRequestCommands(CreatePullRequest("Open").Object).ToList();

        Assert.AreEqual(3, commands.Count);
        Assert.AreEqual("Commands_MergePullRequest", commands[0].Command!.Name);
        Assert.AreEqual("Commands_ClosePullRequest", commands[1].Command!.Name);
        Assert.IsTrue(commands[0].IsCritical);
        Assert.IsTrue(commands[1].IsCritical);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GetPullRequestCommands_ClosedPullRequest_ReturnsReopenCommand()
    {
        var (factory, _, _) = CreateFactory();
        var commands = factory.GetPullRequestCommands(CreatePullRequest("Closed").Object).ToList();

        Assert.AreEqual(2, commands.Count);
        Assert.AreEqual("Commands_ReopenPullRequest", commands[0].Command!.Name);
        Assert.IsFalse(commands[0].IsCritical);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void MergeCommand_Invoke_DefersToConfirmationWithoutCallingManager()
    {
        var (factory, manager, _) = CreateFactory();
        var pr = CreatePullRequest("Open");

        var commands = factory.GetPullRequestCommands(pr.Object).ToList();
        var merge = (Microsoft.CommandPalette.Extensions.Toolkit.InvokableCommand)commands[0].Command!;
        var result = merge.Invoke();

        Assert.IsNotNull(result);
        manager.Verify(x => x.MergePullRequestAsync(It.IsAny<IPullRequest>()), Times.Never);
    }
}
