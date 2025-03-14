// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Dapper.Contrib.Extensions;
using GitHubExtension.DataModel;
using GitHubExtension.DataModel.DataObjects;
using GitHubExtension.Helpers;
using GitHubExtension.Test.Helpers;

namespace GitHubExtension.Test.DataStoreTests;

public partial class DataStoreTests
{
    [TestMethod]
    [TestCategory("Unit")]
    public void DeleteUnreferencedIssues()
    {
        using var dataStore = new DataStore("TestStore", TestHelpers.GetDataStoreFilePath(TestOptions), TestOptions.DataStoreOptions.DataStoreSchema!);
        Assert.IsNotNull(dataStore);
        dataStore.Create();
        Assert.IsNotNull(dataStore.Connection);

        using var tx = dataStore.Connection!.BeginTransaction();

        var now = DateTime.UtcNow.ToDataStoreInteger();

        // Add User record
        dataStore.Connection.Insert(new User { Login = "Kittens", InternalId = 16, AvatarUrl = "https://www.microsoft.com", Type = "Cat" });

        // Add repository record
        dataStore.Connection.Insert(new Repository { OwnerId = 1, InternalId = 47, Name = "TestRepo1", Description = "Short Desc", HtmlUrl = "https://www.microsoft.com", DefaultBranch = "main", HasIssues = 1 });

        var issues = new List<Issue>
        {
            {
                new Issue
                {
                    AuthorId = 1,
                    Number = 1111,
                    InternalId = 18,
                    Title = "No worky",
                    Body = "This feature doesn't work.",
                    HtmlUrl = "https://www.microsoft.com",
                    RepositoryId = 1,
                }
            },
            {
                new Issue
                {
                    AuthorId = 1,
                    Number = 47,
                    InternalId = 20,
                    Title = "Missing Tests",
                    Body = "More tests needed.",
                    HtmlUrl = "https://www.microsoft.com",
                    RepositoryId = 1,
                }
            },
        };
        dataStore.Connection.Insert(issues[0]);
        dataStore.Connection.Insert(issues[1]);

        dataStore.Connection.Insert(new Search { Name = "Hello", SearchString = "Shooting star", TimeUpdated = now });

        dataStore.Connection.Insert(new SearchIssue { Issue = 2, Search = 1 });

        Issue.DeleteNotReferencedBySearch(dataStore);
        tx.Commit();

        // Verify retrieval and input into data objects.
        var dataStoreIssues = dataStore.Connection.GetAll<Issue>().ToList();
        Assert.AreEqual(1, dataStoreIssues.Count);
        foreach (var issue in dataStoreIssues)
        {
            // Get User  and Repo info
            var user = dataStore.Connection.Get<User>(issue.AuthorId);
            var repo = dataStore.Connection.Get<Repository>(issue.RepositoryId);

            TestContext?.WriteLine($"  User: {user.Login}  Repo: {repo.Name} - {issue.Number} - {issue.Title}");
            Assert.AreEqual("Kittens", user.Login);
            Assert.AreEqual("TestRepo1", repo.Name);
            Assert.IsTrue(issue.Id == 2);

            Assert.AreEqual("Kittens", user.Login);
            Assert.AreEqual(47, issue.Number);
            Assert.AreEqual("TestRepo1", repo.Name);
            Assert.AreEqual("Missing Tests", issue.Title);
        }
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void DeleteUnreferencedPullRequests()
    {
        using var dataStore = new DataStore("TestStore", TestHelpers.GetDataStoreFilePath(TestOptions), TestOptions.DataStoreOptions.DataStoreSchema!);
        Assert.IsNotNull(dataStore);
        dataStore.Create();
        Assert.IsNotNull(dataStore.Connection);

        using var tx = dataStore.Connection!.BeginTransaction();
        var now = DateTime.UtcNow.ToDataStoreInteger();

        // Add User record
        dataStore.Connection.Insert(new User { Login = "Kittens", InternalId = 16, AvatarUrl = "https://www.microsoft.com", Type = "Cat" });

        // Add repository record
        dataStore.Connection.Insert(new Repository { OwnerId = 1, InternalId = 47, Name = "TestRepo1", Description = "Short Desc", HtmlUrl = "https://www.microsoft.com", DefaultBranch = "main", HasIssues = 1 });

        var prs = new List<PullRequest>
        {
            {
                new PullRequest
                {
                    AuthorId = 1,
                    Number = 1111,
                    InternalId = 18,
                    Title = "No worky",
                    Body = "This feature doesn't work.",
                    HtmlUrl = "https://www.microsoft.com",
                    RepositoryId = 1,
                }
            },
            {
                new PullRequest
                {
                    AuthorId = 1,
                    Number = 47,
                    InternalId = 20,
                    Title = "Missing Tests",
                    Body = "More tests needed.",
                    HtmlUrl = "https://www.microsoft.com",
                    RepositoryId = 1,
                }
            },
        };

        dataStore.Connection.Insert(prs[0]);
        dataStore.Connection.Insert(prs[1]);

        dataStore.Connection.Insert(new Search { Name = "Hello", SearchString = "Shooting star", TimeUpdated = now });

        dataStore.Connection.Insert(new SearchPullRequest { PullRequest = 2, Search = 1 });

        PullRequest.DeleteNotReferencedBySearch(dataStore);

        tx.Commit();

        // Verify retrieval and input into data objects.
        var dataStorePrs = dataStore.Connection.GetAll<PullRequest>().ToList();
        Assert.AreEqual(1, dataStorePrs.Count);
        foreach (var pr in dataStorePrs)
        {
            // Get User  and Repo info
            var user = dataStore.Connection.Get<User>(pr.AuthorId);
            var repo = dataStore.Connection.Get<Repository>(pr.RepositoryId);

            TestContext?.WriteLine($"  User: {user.Login}  Repo: {repo.Name} - {pr.Number} - {pr.Title}");
            Assert.AreEqual("Kittens", user.Login);
            Assert.AreEqual("TestRepo1", repo.Name);
            Assert.IsTrue(pr.Id == 2);

            Assert.AreEqual("Kittens", user.Login);
            Assert.AreEqual(47, pr.Number);
            Assert.AreEqual("TestRepo1", repo.Name);
            Assert.AreEqual("Missing Tests", pr.Title);
        }
    }
}
