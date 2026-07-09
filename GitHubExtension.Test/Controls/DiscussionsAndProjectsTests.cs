// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Nodes;
using GitHubExtension.Client;
using GitHubExtension.Controls;
using GitHubExtension.Controls.Pages;
using GitHubExtension.DataManager;
using GitHubExtension.DataManager.Data;
using GitHubExtension.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Moq;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class DiscussionsAndProjectsTests
{
    private static Mock<IResources> CreateResources()
    {
        var resources = new Mock<IResources>();
        resources.Setup(x => x.GetResource(It.IsAny<string>(), null)).Returns<string, object>((key, _) => key);
        return resources;
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ResolveGraphQLEndpoint_GitHubCom_ReturnsApiGraphQL()
    {
        var endpoint = GitHubGraphQLClient.ResolveGraphQLEndpoint(new Uri("https://api.github.com/"));
        Assert.AreEqual("https://api.github.com/graphql", endpoint.ToString());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ResolveGraphQLEndpoint_EnterpriseServer_ReturnsHostApiGraphQL()
    {
        var endpoint = GitHubGraphQLClient.ResolveGraphQLEndpoint(new Uri("https://ghe.example.com/api/v3/"));
        Assert.AreEqual("https://ghe.example.com/api/graphql", endpoint.ToString());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task DiscussionsDataManager_MapsSearchNodes()
    {
        var data = JsonNode.Parse(@"{
          ""search"": {
            ""nodes"": [
              { ""title"": ""First"", ""url"": ""https://github.com/o/r/discussions/1"", ""number"": 1, ""updatedAt"": ""2024-01-01T00:00:00Z"", ""repository"": { ""nameWithOwner"": ""o/r"" }, ""author"": { ""login"": ""octocat"" } },
              { ""title"": ""Second"", ""url"": ""https://github.com/o/r/discussions/2"", ""number"": 2, ""updatedAt"": ""2024-01-02T00:00:00Z"", ""repository"": { ""nameWithOwner"": ""o/r"" }, ""author"": { ""login"": ""hubber"" } }
            ]
          }
        }");

        var client = new Mock<IGitHubGraphQLClient>();
        client.Setup(x => x.QueryAsync(It.IsAny<string>(), It.IsAny<object?>())).ReturnsAsync(data);

        var manager = new DiscussionsDataManager(client.Object);
        var discussions = (await manager.SearchDiscussionsAsync("author:@me")).ToList();

        Assert.AreEqual(2, discussions.Count);
        Assert.AreEqual("First", discussions[0].Title);
        Assert.AreEqual("o/r", discussions[0].RepositoryFullName);
        Assert.AreEqual("octocat", discussions[0].Author);
        Assert.AreEqual(2, discussions[1].Number);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task DiscussionsDataManager_NoNodes_ReturnsEmpty()
    {
        var client = new Mock<IGitHubGraphQLClient>();
        client.Setup(x => x.QueryAsync(It.IsAny<string>(), It.IsAny<object?>())).ReturnsAsync(JsonNode.Parse("{}"));

        var manager = new DiscussionsDataManager(client.Object);
        var discussions = await manager.SearchDiscussionsAsync("author:@me");

        Assert.AreEqual(0, discussions.Count());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task ProjectsDataManager_MapsViewerProjects()
    {
        var data = JsonNode.Parse(@"{
          ""viewer"": {
            ""projectsV2"": {
              ""nodes"": [
                { ""title"": ""Roadmap"", ""url"": ""https://github.com/users/o/projects/1"", ""number"": 1, ""closed"": false },
                { ""title"": ""Archive"", ""url"": ""https://github.com/users/o/projects/2"", ""number"": 2, ""closed"": true }
              ]
            }
          }
        }");

        var client = new Mock<IGitHubGraphQLClient>();
        client.Setup(x => x.QueryAsync(It.IsAny<string>(), It.IsAny<object?>())).ReturnsAsync(data);

        var manager = new ProjectsDataManager(client.Object);
        var projects = (await manager.GetMyProjectsAsync()).ToList();

        Assert.AreEqual(2, projects.Count);
        Assert.AreEqual("Roadmap", projects[0].Title);
        Assert.IsFalse(projects[0].Closed);
        Assert.IsTrue(projects[1].Closed);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task AutoMergeManager_Enable_ResolvesIdThenMutates()
    {
        var idData = JsonNode.Parse(@"{ ""repository"": { ""pullRequest"": { ""id"": ""PR_kabc123"" } } }");

        var client = new Mock<IGitHubGraphQLClient>();
        var capturedIds = new List<string?>();
        client.Setup(x => x.QueryAsync(It.Is<string>(q => q.Contains("pullRequest(number")), It.IsAny<object?>()))
            .ReturnsAsync(idData);
        client.Setup(x => x.QueryAsync(It.Is<string>(q => q.Contains("enablePullRequestAutoMerge")), It.IsAny<object?>()))
            .Callback<string, object?>((_, vars) =>
            {
                var dict = (IDictionary<string, object?>)vars!;
                capturedIds.Add(dict["id"]?.ToString());
            })
            .ReturnsAsync((JsonNode?)null);

        var pr = new Mock<IPullRequest>();
        pr.Setup(x => x.HtmlUrl).Returns("https://github.com/owner/repo/pull/7");
        pr.Setup(x => x.Number).Returns(7);

        var manager = new GitHubAutoMergeManager(client.Object);
        await manager.EnableAutoMergeAsync(pr.Object);

        var expectedIds = new[] { "PR_kabc123" };
        CollectionAssert.AreEqual(expectedIds, capturedIds);
        client.Verify(x => x.QueryAsync(It.Is<string>(q => q.Contains("enablePullRequestAutoMerge")), It.IsAny<object?>()), Times.Once);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void DiscussionsPage_EmptyQuery_ShowsPrompt()
    {
        var manager = new Mock<IDiscussionsDataManager>();
        var page = new DiscussionsPage(manager.Object, CreateResources().Object, "Search discussions", string.Empty);

        var items = page.GetItems();

        Assert.AreEqual(1, items.Length);
        Assert.AreEqual("Pages_Discussions_Prompt", items[0].Title);
        manager.Verify(x => x.SearchDiscussionsAsync(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void DiscussionsPage_MyDiscussions_UsesBaseQuery()
    {
        var manager = new Mock<IDiscussionsDataManager>();
        manager.Setup(x => x.SearchDiscussionsAsync(It.IsAny<string>()))
            .ReturnsAsync(new[] { new Discussion { Title = "D", HtmlUrl = "https://github.com/o/r/discussions/1" } });

        var page = new DiscussionsPage(manager.Object, CreateResources().Object, "My discussions", "author:@me");

        var items = page.GetItems();

        Assert.AreEqual(1, items.Length);
        manager.Verify(x => x.SearchDiscussionsAsync("author:@me"), Times.Once);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ProjectsPage_WithProjects_ReturnsItems()
    {
        var manager = new Mock<IProjectsDataManager>();
        manager.Setup(x => x.GetMyProjectsAsync())
            .ReturnsAsync(new[]
            {
                new Project { Title = "A", HtmlUrl = "https://github.com/users/o/projects/1", Closed = false },
                new Project { Title = "B", HtmlUrl = "https://github.com/users/o/projects/2", Closed = true },
            });

        var page = new ProjectsPage(manager.Object, CreateResources().Object);

        var items = page.GetItems();

        Assert.AreEqual(2, items.Length);
    }
}
