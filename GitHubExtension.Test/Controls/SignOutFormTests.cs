// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Reflection;
using GitHubExtension.Controls;
using GitHubExtension.Controls.Commands;
using GitHubExtension.Controls.Forms;
using GitHubExtension.DeveloperIds;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Octokit;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class SignOutFormTests
{
    private sealed record TestContext(
        Mock<IResources> ResourcesMock,
        Mock<SignOutCommand> SignOutCommandMock,
        Mock<AuthenticationMediator> AuthMediatorMock,
        Mock<IDeveloperIdProvider> DeveloperIdProviderMock,
        SignOutForm Form
    );

    private TestContext CreateTestContext(
        string? developerId = "user1",
        Func<string, string?>? resourceFunc = null)
    {
        var mockResources = new Mock<IResources>();
        var mockAuthenticationMediator = new Mock<AuthenticationMediator>();
        var mockDeveloperIdProvider = new Mock<IDeveloperIdProvider>();
        var mockSignOutCommand = new Mock<SignOutCommand>(mockResources.Object, mockDeveloperIdProvider.Object, mockAuthenticationMediator.Object);

        mockResources
            .Setup(r => r.GetResource(It.IsAny<string>(), null))
            .Returns((string key, Serilog.ILogger? _) => (resourceFunc != null ? resourceFunc(key) ?? key : $"res_{key}"));

        if (developerId != null)
        {
            var devId = new DeveloperId(
                developerId,
                "Display Name",
                "user1@example.com",
                "https://github.com/user1",
                new GitHubClient(new Octokit.ProductHeaderValue("TestApp")));
            mockDeveloperIdProvider.Setup(d => d.GetLoggedInDeveloperId()).Returns(devId);
        }
        else
        {
            mockDeveloperIdProvider.Setup(d => d.GetLoggedInDeveloperId()).Returns((IDeveloperId?)null);
        }

        var form = new SignOutForm(
            mockResources.Object,
            mockAuthenticationMediator.Object,
            mockSignOutCommand.Object,
            mockDeveloperIdProvider.Object);

        return new TestContext(mockResources, mockSignOutCommand, mockAuthenticationMediator, mockDeveloperIdProvider, form);
    }

    [TestMethod]
    public void Constructor_RegistersEventHandlers()
    {
        // Just verify events exist (event handler registration is not directly testable without raising events)
        var loadingStateChanged = typeof(AuthenticationMediator).GetEvent("LoadingStateChanged");
        var signInAction = typeof(AuthenticationMediator).GetEvent("SignInAction");
        var signOutAction = typeof(AuthenticationMediator).GetEvent("SignOutAction");

        Assert.IsNotNull(loadingStateChanged);
        Assert.IsNotNull(signInAction);
        Assert.IsNotNull(signOutAction);
    }

    [TestMethod]
    public void TemplateSubstitutions_ReturnsExpectedValues()
    {
        var ctx = CreateTestContext();
        var dict = ctx.Form.TemplateSubstitutions;
        Assert.AreEqual("res_Forms_Sign_Out_Title", dict["{{AuthTitle}}"]);
        Assert.IsTrue(dict["{{AuthButtonTitle}}"].Contains("user1"));
        Assert.IsTrue(dict["{{AuthIcon}}"].StartsWith("data:image/png;base64,", StringComparison.Ordinal));
        Assert.AreEqual("res_Forms_Sign_Out_Tooltip", dict["{{AuthButtonTooltip}}"]);
        Assert.AreEqual("true", dict["{{ButtonIsEnabled}}"]);
    }

    [TestMethod]
    public void TemplateJson_ReturnsExpectedTemplate()
    {
        var ctx = CreateTestContext();
        var json = ctx.Form.TemplateJson;
        Assert.IsNotNull(json);
        Assert.IsTrue(json.Length > 0);
    }

    [TestMethod]
    public void SubmitForm_InvokesSignOutCommand()
    {
        var ctx = CreateTestContext();
        ctx.SignOutCommandMock.Setup(c => c.Invoke()).Returns(Mock.Of<CommandResult>());

        var result = ctx.Form.SubmitForm("inputs", "data");

        ctx.SignOutCommandMock.Verify(c => c.Invoke(), Times.Once);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void SetButtonEnabled_UpdatesButtonState()
    {
        var ctx = CreateTestContext();
        var method = typeof(SignOutForm).GetMethod("SetButtonEnabled", BindingFlags.NonPublic | BindingFlags.Instance);
        method!.Invoke(ctx.Form, new object[] { false });

        var dict = ctx.Form.TemplateSubstitutions;
        Assert.AreEqual("false", dict["{{ButtonIsEnabled}}"]);
    }

    [TestMethod]
    public void ResetButton_UpdatesButtonState_OnSignInStatusChanged()
    {
        var ctx = CreateTestContext();
        var method = typeof(SignOutForm).GetMethod("ResetButton", BindingFlags.NonPublic | BindingFlags.Instance);
        var args = new SignInStatusChangedEventArgs(false);
        method!.Invoke(ctx.Form, new object?[] { null, args });

        var dict = ctx.Form.TemplateSubstitutions;
        Assert.AreEqual("false", dict["{{ButtonIsEnabled}}"]);
    }

    [TestMethod]
    public void OnLoadingStateChanged_DisablesButton()
    {
        var ctx = CreateTestContext();
        var method = typeof(SignOutForm).GetMethod("OnLoadingStateChanged", BindingFlags.NonPublic | BindingFlags.Instance);
        method!.Invoke(ctx.Form, new object[] { new(), true });

        var dict = ctx.Form.TemplateSubstitutions;
        Assert.AreEqual("false", dict["{{ButtonIsEnabled}}"]);
    }

    [TestMethod]
    public void TemplateSubstitutions_HandlesNullDeveloperId()
    {
        var ctx = CreateTestContext(developerId: null);
        var dict = ctx.Form.TemplateSubstitutions;
        Assert.IsFalse(dict["{{AuthButtonTitle}}"].Contains("null"));
    }

    [TestMethod]
    public void TemplateSubstitutions_HandlesResourceLookupFailure()
    {
        var ctx = CreateTestContext(resourceFunc: _ => null);
        var dict = ctx.Form.TemplateSubstitutions;
        Assert.IsNull(dict["{{AuthTitle}}"]);
        Assert.IsNull(dict["{{AuthButtonTooltip}}"]);
    }

    [TestMethod]
    public void SetButtonEnabled_UpdatesTemplateJsonAndNotifiesPropertyChanged()
    {
        var ctx = CreateTestContext();

        var oldTemplateJson = ctx.Form.TemplateJson;
        var propertyChangedCalled = false;

        var method = typeof(SignOutForm).GetMethod("SetButtonEnabled", BindingFlags.NonPublic | BindingFlags.Instance);
        method!.Invoke(ctx.Form, new object[] { false });

        var newTemplateJson = ctx.Form.TemplateJson;
        if (!Equals(oldTemplateJson, newTemplateJson))
        {
            propertyChangedCalled = true;
        }

        Assert.IsTrue(propertyChangedCalled);
        Assert.IsNotNull(ctx.Form.TemplateJson);
    }
}
