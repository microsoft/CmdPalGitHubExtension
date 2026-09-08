// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection;
using GitHubExtension.Client;
using GitHubExtension.Controls.Pages;
using GitHubExtension.DeveloperIds;
using GitHubExtension.Helpers;
using Moq;
using Octokit;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class CodespacesPageTest
{
    [TestMethod]
    [TestCategory("Unit")]
    public void CodespacesPageCreate_NotNull()
    {
        var resources = CreateResources();
        var page = new CodespacesPage(resources.Object, CreateGitHubClientProvider(CreateCodespacesCollection(Array.Empty<Codespace>())));

        Assert.IsNotNull(page);
        Assert.IsTrue(page.IsLoading);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void CodespacesPageGetItems_ReturnsCodespacesFromClient()
    {
        var resources = CreateResources();
        var codespace = CreateCodespace("noraa/test-repo", "noraa-test", "Standard Linux", "Available", "https://github.com/codespaces/abc123");
        var page = new CodespacesPage(resources.Object, CreateGitHubClientProvider(CreateCodespacesCollection([codespace])));

        var previousPath = Environment.GetEnvironmentVariable("PATH");
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            Directory.CreateDirectory(tempDirectory);
            File.WriteAllText(Path.Combine(tempDirectory, "code"), string.Empty);
            File.WriteAllText(Path.Combine(tempDirectory, "code-insiders"), string.Empty);
            Environment.SetEnvironmentVariable("PATH", tempDirectory);

            var items = page.GetItems();

            Assert.AreEqual(1, items.Length);
            Assert.AreEqual("noraa/test-repo", items[0].Title);
            Assert.AreEqual("noraa-test - Standard Linux (Available)", items[0].Subtitle);
            Assert.AreEqual(2, items[0].MoreCommands.Length);
            Assert.IsFalse(page.IsLoading);
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", previousPath);
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
        }
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void CodespacesPageGetItems_WhenCodespacesApiFails_ReturnsErrorItem()
    {
        var resources = CreateResources();
        var mockCodespacesClient = new Mock<ICodespacesClient>();
        mockCodespacesClient.Setup(x => x.GetAll()).ThrowsAsync(new InvalidOperationException("Test failure"));

        var page = new CodespacesPage(resources.Object, CreateGitHubClientProvider(mockCodespacesClient.Object));

        var items = page.GetItems();

        Assert.AreEqual(1, items.Length);
        Assert.AreEqual("Codespaces failed", items[0].Title);
        Assert.AreEqual("Unable to load codespaces", items[0].Subtitle);
        Assert.IsFalse(page.IsLoading);
    }

    private static Mock<IResources> CreateResources()
    {
        var resources = new Mock<IResources>();
        resources.Setup(x => x.GetResource("Pages_Codespaces", null)).Returns("Codespaces");
        resources.Setup(x => x.GetResource("Pages_Codespaces_Placeholder", null)).Returns("Search codespaces");
        resources.Setup(x => x.GetResource("Commands_Open_VS_Code", null)).Returns("Open in VS Code");
        resources.Setup(x => x.GetResource("Commands_Open_VS_Code_Insiders", null)).Returns("Open in VS Code Insiders");
        resources.Setup(x => x.GetResource("Message_Codespaces_LoadFail", null)).Returns("Codespaces failed");
        resources.Setup(x => x.GetResource("Message_Codespaces_LoadFail_Description", null)).Returns("Unable to load codespaces");
        return resources;
    }

    private static GitHubClientProvider CreateGitHubClientProvider(CodespacesCollection collection)
    {
        var codespacesClient = new Mock<ICodespacesClient>();
        codespacesClient.Setup(x => x.GetAll()).ReturnsAsync(collection);

        return CreateGitHubClientProvider(codespacesClient.Object);
    }

    private static GitHubClientProvider CreateGitHubClientProvider(ICodespacesClient codespacesClient)
    {
        var gitHubClient = new Mock<IGitHubClient>();
        gitHubClient.Setup(x => x.Codespaces).Returns(codespacesClient);

        var developerId = new Mock<IDeveloperId>();
        developerId.Setup(x => x.LoginId).Returns("octocat");
        developerId.Setup(x => x.GitHubClient).Returns(gitHubClient.Object);

        var developerIdProvider = new Mock<IDeveloperIdProvider>();
        developerIdProvider.Setup(x => x.GetLoggedInDeveloperIdsInternal()).Returns([developerId.Object]);

        return new GitHubClientProvider(developerIdProvider.Object);
    }

    private static CodespacesCollection CreateCodespacesCollection(IReadOnlyList<Codespace> codespaces)
    {
        return new CodespacesCollection(codespaces, codespaces.Count);
    }

    private static Codespace CreateCodespace(string repositoryFullName, string name, string machineDisplayName, string state, string webUrl)
    {
        var repositoryParts = repositoryFullName.Split('/');
        var owner = new User();
        SetPrivateSetProperty(owner, nameof(User.Login), repositoryParts[0]);

        var repository = new Repository();
        SetPrivateSetProperty(repository, nameof(Repository.Owner), owner);
        SetPrivateSetProperty(repository, nameof(Repository.Name), repositoryParts[1]);

        var machine = new Machine();
        SetPrivateSetProperty(machine, nameof(Machine.DisplayName), machineDisplayName);

        var codespace = new Codespace();
        SetPrivateSetProperty(codespace, nameof(Codespace.Name), name);
        SetPrivateSetProperty(codespace, nameof(Codespace.Repository), repository);
        SetPrivateSetProperty(codespace, nameof(Codespace.Machine), machine);
        SetPrivateSetProperty(codespace, nameof(Codespace.State), new StringEnum<CodespaceState>(state));
        SetPrivateSetProperty(codespace, nameof(Codespace.WebUrl), webUrl);

        return codespace;
    }

    private static void SetPrivateSetProperty<T, TValue>(T target, string propertyName, TValue value)
    {
        var property = typeof(T).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)
            ?? throw new InvalidOperationException($"Property {propertyName} not found on {typeof(T).Name}");
        property.SetValue(target, value);
    }
}
