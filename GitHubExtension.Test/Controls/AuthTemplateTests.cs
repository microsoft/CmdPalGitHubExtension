// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using GitHubExtension.Controls;
using GitHubExtension.Controls.Commands;
using GitHubExtension.Controls.Forms;
using GitHubExtension.DeveloperIds;
using GitHubExtension.Helpers;
using Moq;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class AuthTemplateTests
{
    private static Mock<IResources> CreateResources()
    {
        var mockResources = new Mock<IResources>();
        mockResources
            .Setup(r => r.GetResource(It.IsAny<string>(), null))
            .Returns((string key, Serilog.ILogger? _) => $"res_{key}");
        return mockResources;
    }

    private static SignInForm CreateSignInForm()
    {
        var resources = CreateResources().Object;
        var mediator = new AuthenticationMediator();
        var developerIdProvider = new Mock<IDeveloperIdProvider>().Object;
        var signInCommand = new SignInCommand(resources, developerIdProvider, mediator);

        return new SignInForm(mediator, resources, developerIdProvider, signInCommand);
    }

    private static SignOutForm CreateSignOutForm()
    {
        var resources = CreateResources().Object;
        var mediator = new AuthenticationMediator();
        var developerIdProvider = new Mock<IDeveloperIdProvider>().Object;
        var signOutCommand = new SignOutCommand(resources, developerIdProvider, mediator);

        return new SignOutForm(resources, mediator, signOutCommand, developerIdProvider);
    }

    private static void AssertFullySubstitutedJson(string templateJson)
    {
        StringAssert.DoesNotMatch(templateJson, new System.Text.RegularExpressions.Regex(@"\{\{\w+\}\}"));
        using var document = JsonDocument.Parse(templateJson);
        Assert.IsNotNull(document);
    }

    [TestMethod]
    public void SignInForm_RendersGitHubDotComCardWithoutHostInput()
    {
        using var form = CreateSignInForm();

        var templateJson = form.TemplateJson;

        AssertFullySubstitutedJson(templateJson);
        Assert.IsFalse(templateJson.Contains("EnterpriseHost", StringComparison.Ordinal));
        Assert.IsTrue(templateJson.Contains("showEnterprise", StringComparison.Ordinal));
    }

    [TestMethod]
    public void SignInForm_SwitchesToEnterpriseCardAndBack()
    {
        using var form = CreateSignInForm();

        form.SubmitForm(string.Empty, "{\"action\":\"showEnterprise\"}");

        var enterpriseJson = form.TemplateJson;
        AssertFullySubstitutedJson(enterpriseJson);
        Assert.IsTrue(enterpriseJson.Contains("EnterpriseHost", StringComparison.Ordinal));
        Assert.IsTrue(enterpriseJson.Contains("showGitHubDotCom", StringComparison.Ordinal));

        form.SubmitForm(string.Empty, "{\"action\":\"showGitHubDotCom\"}");

        Assert.IsFalse(form.TemplateJson.Contains("EnterpriseHost", StringComparison.Ordinal));
    }

    [TestMethod]
    public void SignOutForm_RendersFullySubstitutedCard()
    {
        using var form = CreateSignOutForm();

        AssertFullySubstitutedJson(form.TemplateJson);
    }
}
