// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection;
using System.Text.Json;
using GitHubExtension.Controls;
using GitHubExtension.Controls.Commands;
using GitHubExtension.Controls.Forms;
using GitHubExtension.DeveloperIds;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Moq;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class SignOutFormTests
{
    private sealed record TestContext(
        Mock<IResources> ResourcesMock,
        Mock<SignOutCommand> SignOutCommandMock,
        Mock<AuthenticationMediator> AuthMediatorMock,
        Mock<IDeveloperIdProvider> DeveloperIdProviderMock,
        SignOutForm Form,
        string AssetsPath
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
                new Octokit.GitHubClient(new Octokit.ProductHeaderValue("TestApp")));
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

        var assetsPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "Assets"));

        return new TestContext(mockResources, mockSignOutCommand, mockAuthenticationMediator, mockDeveloperIdProvider, form, assetsPath);
    }

    [TestMethod]
    public void Constructor_RegistersEventHandlers()
    {
        Assert.IsNotNull(typeof(AuthenticationMediator).GetEvent("LoadingStateChanged"));
        Assert.IsNotNull(typeof(AuthenticationMediator).GetEvent("SignInAction"));
        Assert.IsNotNull(typeof(AuthenticationMediator).GetEvent("SignOutAction"));
    }

    [DataRow("user1", "res_Forms_Sign_Out_Title", true, "res_Forms_Sign_Out_Tooltip", true)]
    [DataRow(null, "res_Forms_Sign_Out_Title", false, "res_Forms_Sign_Out_Tooltip", true)]
    [TestMethod]
    public void TemplateSubstitutions_ReturnsExpectedValues(string? developerId, string expectedTitle, bool expectUserName, string expectedTooltip, bool expectedButtonEnabled)
    {
        var ctx = CreateTestContext(developerId: developerId);
        var dict = ctx.Form.TemplateSubstitutions;

        // All values are now JSON-serialized, so we must deserialize to check the actual value
        Assert.AreEqual(expectedTitle, JsonSerializer.Deserialize<string>(dict["{{AuthTitle}}"]));
        if (expectUserName)
        {
            Assert.IsTrue(JsonSerializer.Deserialize<string>(dict["{{AuthButtonTitle}}"])?.Contains("user1"));
        }

        Assert.IsTrue(JsonSerializer.Deserialize<string>(dict["{{AuthIcon}}"])?.StartsWith("data:image/png;base64,", StringComparison.Ordinal));
        Assert.AreEqual(expectedTooltip, JsonSerializer.Deserialize<string>(dict["{{AuthButtonTooltip}}"]));

        Assert.AreEqual(expectedButtonEnabled, JsonSerializer.Deserialize<bool>(dict["{{ButtonIsEnabled}}"]));
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

    [DataRow(false, false)]
    [DataRow(true, true)]
    [TestMethod]
    public void SetButtonEnabled_UpdatesButtonState(bool isEnabled, bool expected)
    {
        var ctx = CreateTestContext();
        var method = typeof(SignOutForm).GetMethod("SetButtonEnabled", BindingFlags.NonPublic | BindingFlags.Instance);
        method!.Invoke(ctx.Form, new object[] { isEnabled });

        var dict = ctx.Form.TemplateSubstitutions;
        Assert.AreEqual(expected, JsonSerializer.Deserialize<bool>(dict["{{ButtonIsEnabled}}"]));
    }

    [TestMethod]
    public void ResetButton_UpdatesButtonState_OnSignInStatusChanged()
    {
        var ctx = CreateTestContext();
        var method = typeof(SignOutForm).GetMethod("ResetButton", BindingFlags.NonPublic | BindingFlags.Instance);
        var args = new SignInStatusChangedEventArgs(false);
        method!.Invoke(ctx.Form, new object?[] { null, args });

        var dict = ctx.Form.TemplateSubstitutions;
        Assert.AreEqual(false, JsonSerializer.Deserialize<bool>(dict["{{ButtonIsEnabled}}"]));
    }

    [TestMethod]
    public void OnLoadingStateChanged_DisablesButton()
    {
        var ctx = CreateTestContext();
        var method = typeof(SignOutForm).GetMethod("OnLoadingStateChanged", BindingFlags.NonPublic | BindingFlags.Instance);
        method!.Invoke(ctx.Form, new object[] { new(), true });

        var dict = ctx.Form.TemplateSubstitutions;
        Assert.AreEqual(false, JsonSerializer.Deserialize<bool>(dict["{{ButtonIsEnabled}}"]));
    }

    [TestMethod]
    public void TemplateSubstitutions_HandlesResourceLookupFailure()
    {
        var ctx = CreateTestContext(resourceFunc: _ => null);
        var dict = ctx.Form.TemplateSubstitutions;
        Assert.AreEqual("Forms_Sign_Out_Title", JsonSerializer.Deserialize<string>(dict["{{AuthTitle}}"]));
        Assert.AreEqual("Forms_Sign_Out_Tooltip", JsonSerializer.Deserialize<string>(dict["{{AuthButtonTooltip}}"]));
    }

    [TestMethod]
    public void SetButtonEnabled_UpdatesTemplateJsonAndNotifiesPropertyChanged()
    {
        var ctx = CreateTestContext();

        var oldTemplateJson = ctx.Form.TemplateJson;
        var method = typeof(SignOutForm).GetMethod("SetButtonEnabled", BindingFlags.NonPublic | BindingFlags.Instance);
        method!.Invoke(ctx.Form, new object[] { false });

        var newTemplateJson = ctx.Form.TemplateJson;
        Assert.AreNotEqual(oldTemplateJson, newTemplateJson);
        Assert.IsNotNull(ctx.Form.TemplateJson);
    }
}
