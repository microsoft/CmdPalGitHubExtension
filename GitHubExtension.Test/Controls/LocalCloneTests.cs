// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using GitHubExtension.Controls.Forms;
using GitHubExtension.DataManager;
using GitHubExtension.DataManager.Data;
using GitHubExtension.Helpers;
using Moq;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class LocalCloneTests
{
    private static Mock<IResources> CreateResources()
    {
        var resources = new Mock<IResources>();
        resources.Setup(x => x.GetResource(It.IsAny<string>(), null)).Returns<string, object>((key, _) => key);
        return resources;
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ResolveCloneUrl_OwnerRepo_BuildsHttpsUrl()
    {
        var (cloneUrl, name) = RepositoryCloneManager.ResolveCloneUrl("octocat/Hello-World");

        Assert.AreEqual("https://github.com/octocat/Hello-World.git", cloneUrl);
        Assert.AreEqual("Hello-World", name);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ResolveCloneUrl_FullUrl_UsedAsIsAndStripsGitSuffix()
    {
        var (cloneUrl, name) = RepositoryCloneManager.ResolveCloneUrl("https://github.com/octocat/Hello-World.git");

        Assert.AreEqual("https://github.com/octocat/Hello-World.git", cloneUrl);
        Assert.AreEqual("Hello-World", name);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ResolveCloneUrl_HtmlUrl_DerivesRepositoryName()
    {
        var (cloneUrl, name) = RepositoryCloneManager.ResolveCloneUrl("https://github.com/octocat/Hello-World");

        Assert.AreEqual("https://github.com/octocat/Hello-World", cloneUrl);
        Assert.AreEqual("Hello-World", name);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task CloneRepositoryAsync_GitMissing_Throws()
    {
        var gitService = new Mock<IGitService>();
        gitService.Setup(x => x.IsGitInstalled()).Returns(false);
        var settingsStore = new Mock<ICloneSettingsStore>();
        var manager = new RepositoryCloneManager(gitService.Object, settingsStore.Object);

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => manager.CloneRepositoryAsync("octocat/Hello-World"));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task CloneRepositoryAsync_UsesConfiguredBaseDirectory_AndReturnsDestination()
    {
        var baseDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var gitService = new Mock<IGitService>();
        gitService.Setup(x => x.IsGitInstalled()).Returns(true);
        string? capturedUrl = null;
        string? capturedDestination = null;
        gitService
            .Setup(x => x.CloneAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((url, destination, _) =>
            {
                capturedUrl = url;
                capturedDestination = destination;
            })
            .Returns(Task.CompletedTask);
        var settingsStore = new Mock<ICloneSettingsStore>();
        settingsStore.Setup(x => x.GetCloneBaseDirectoryAsync()).ReturnsAsync(baseDirectory);
        var manager = new RepositoryCloneManager(gitService.Object, settingsStore.Object);

        var result = await manager.CloneRepositoryAsync("octocat/Hello-World");

        Assert.AreEqual(Path.Combine(baseDirectory, "Hello-World"), result);
        Assert.AreEqual("https://github.com/octocat/Hello-World.git", capturedUrl);
        Assert.AreEqual(Path.Combine(baseDirectory, "Hello-World"), capturedDestination);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task CloneRepositoryAsync_ExplicitTargetDirectory_OverridesSettings()
    {
        var explicitDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var gitService = new Mock<IGitService>();
        gitService.Setup(x => x.IsGitInstalled()).Returns(true);
        gitService
            .Setup(x => x.CloneAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var settingsStore = new Mock<ICloneSettingsStore>();
        var manager = new RepositoryCloneManager(gitService.Object, settingsStore.Object);

        var result = await manager.CloneRepositoryAsync("octocat/Hello-World", explicitDirectory);

        Assert.AreEqual(Path.Combine(explicitDirectory, "Hello-World"), result);
        settingsStore.Verify(x => x.GetCloneBaseDirectoryAsync(), Times.Never);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void CloneSettingsStore_DefaultBaseDirectory_IsUnderUserProfile()
    {
        var store = new CloneSettingsStore();

        var defaultDirectory = store.GetDefaultCloneBaseDirectory();

        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        Assert.AreEqual(Path.Combine(userProfile, "source", "repos"), defaultDirectory);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void CloneRepositoryForm_RendersValidTemplateJson()
    {
        var resources = CreateResources();
        var cloneManager = new Mock<IRepositoryCloneManager>();
        var settingsStore = new Mock<ICloneSettingsStore>();
        settingsStore.Setup(x => x.GetDefaultCloneBaseDirectory()).Returns("C:\\src");

        var form = new CloneRepositoryForm(cloneManager.Object, settingsStore.Object, resources.Object);

        var json = form.TemplateJson;
        Assert.IsFalse(string.IsNullOrWhiteSpace(json));
        using var document = JsonDocument.Parse(json);
        Assert.AreEqual("AdaptiveCard", document.RootElement.GetProperty("type").GetString());
    }
}
