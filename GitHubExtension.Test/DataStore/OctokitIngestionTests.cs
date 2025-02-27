// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Dapper.Contrib.Extensions;
using GitHubExtension.Client;
using GitHubExtension.DataModel;
using GitHubExtension.DeveloperId;
using Octokit;

namespace GitHubExtension.Test;

public partial class DataStoreTests
{
    [TestMethod]
    [TestCategory("LiveData")]
    public void AddUserFromOctokit()
    {
        using var dataStore = new DataStore("TestStore", TestHelpers.GetDataStoreFilePath(TestOptions), TestOptions.DataStoreOptions.DataStoreSchema!);
        Assert.IsNotNull(dataStore);
        dataStore.Create();
        Assert.IsNotNull(dataStore.Connection);

        var items = new List<string>
        {
            "microsoft",
            "github",
            "octokit",
        };

        var client = new GitHubClientProvider(new DeveloperIdProvider()).GetClient();
        using var tx = dataStore.Connection!.BeginTransaction();
        foreach (var item in items)
        {
            var toInsert = client.User.Get(item).Result;
            var inserted = DataModel.User.GetOrCreateByOctokitUser(dataStore, toInsert);
            Assert.AreNotEqual(DataStore.NoForeignKey, inserted.Id);
        }

        tx.Commit();

        // Verify retrieval and input into data objects.
        var dataStoreUsers = dataStore.Connection.GetAll<DataModel.User>().ToList();
        Assert.AreEqual(items.Count, dataStoreUsers.Count);
        foreach (var user in dataStoreUsers)
        {
            TestContext?.WriteLine($"  User: {user.Id}: {user.InternalId} - {user.Login} - {user.AvatarUrl} - {user.Type}");

            // Ids are inserted in order starting at 1, so they should match the list index - 1.
            var index = (int)user.Id - 1;
            Assert.AreEqual(items[index], user.Login);
        }

        // Test Update
        var updatedUser = DataModel.User.GetById(dataStore, 1);
        Assert.IsNotNull(updatedUser);
        updatedUser.AvatarUrl = "https://some.new.url";
        var afterUpdate = DataModel.User.AddOrUpdateUser(dataStore, updatedUser);
        Assert.IsNotNull(afterUpdate);
        Assert.AreEqual(1, afterUpdate.Id);

        // The update code will have set a timestamp on updating the record, and we will
        // be within that window of don't update. This value should not have updated.
        Assert.AreNotEqual("https://some.new.url", afterUpdate.AvatarUrl);
    }

    [TestMethod]
    [TestCategory("LiveData")]
    public void AddRepositoryFromOctokit()
    {
        using var dataStore = new DataStore("TestStore", TestHelpers.GetDataStoreFilePath(TestOptions), TestOptions.DataStoreOptions.DataStoreSchema!);
        Assert.IsNotNull(dataStore);
        dataStore.Create();
        Assert.IsNotNull(dataStore.Connection);

        var items = new List<Tuple<string, string>>
        {
            Tuple.Create("microsoft", "PowerToys"),
            Tuple.Create("octokit", "octokit.net"),
            Tuple.Create("microsoft", "WindowsAppSDK"),
        };

        var client = new GitHubClientProvider(new DeveloperIdProvider()).GetClient();
        using var tx = dataStore.Connection!.BeginTransaction();
        foreach (var item in items)
        {
            var toInsert = client.Repository.Get(item.Item1, item.Item2).Result;
            var inserted = DataModel.Repository.GetOrCreateByOctokitRepository(dataStore, toInsert);
            Assert.AreNotEqual(DataStore.NoForeignKey, inserted.Id);
        }

        tx.Commit();

        // Verify correct number of entries was added.
        var dataStoreRepos = dataStore.Connection.GetAll<DataModel.Repository>().ToList();
        Assert.AreEqual(items.Count, dataStoreRepos.Count);

        // Verify E2E Issue object works.
        TestContext?.WriteLine($"Repository Data from Data Object only:");
        for (var i = 1; i <= items.Count; i++)
        {
            var repoObj = DataModel.Repository.GetById(dataStore, i);
            Assert.IsNotNull(repoObj);

            // Ids are inserted in order starting at 1, so they should match the list index - 1.
            var index = (int)repoObj.Id - 1;
            Assert.AreEqual(items[index].Item1, repoObj.Owner.Login);
            Assert.AreEqual(items[index].Item2, repoObj.Name);
            Assert.AreEqual(items[index].Item1 + '/' + items[index].Item2, repoObj.FullName);

            TestContext?.WriteLine($"  FullName: {repoObj.FullName}  User: {repoObj.Owner.Login}  Repo: {repoObj.Name} - {repoObj.InternalId} - {repoObj.Description}");
        }

        // Verify other repository accessors.
        TestContext?.WriteLine($"List of all repositories:");
        var allRepositories = DataModel.Repository.GetAll(dataStore);
        foreach (var repoObj in allRepositories)
        {
            TestContext?.WriteLine($"  FullName: {repoObj.FullName}  User: {repoObj.Owner.Login}  Repo: {repoObj.Name} - {repoObj.InternalId} - {repoObj.Description}");
        }

        TestContext?.WriteLine($"Verifying Repository.Get");
        foreach (var item in items)
        {
            var repo1 = DataModel.Repository.Get(dataStore, item.Item1, item.Item2);
            Assert.IsNotNull(repo1);
            var repo2 = DataModel.Repository.Get(dataStore, item.Item1 + '/' + item.Item2);
            Assert.IsNotNull(repo2);
            Assert.AreEqual(repo1.Id, repo2.Id);
            TestContext?.WriteLine($"  FullName: {repo1.FullName}  User: {repo1.Owner.Login}  Repo: {repo1.Name}");
        }
    }

    [TestMethod]
    [TestCategory("LiveData")]
    public void AddPullRequestFromOctokit()
    {
        using var dataStore = new DataStore("TestStore", TestHelpers.GetDataStoreFilePath(TestOptions), TestOptions.DataStoreOptions.DataStoreSchema!);
        Assert.IsNotNull(dataStore);
        dataStore.Create();
        Assert.IsNotNull(dataStore.Connection);

        var repositories = new List<Tuple<string, string>>
        {
            Tuple.Create("microsoft", "PowerToys"),
            Tuple.Create("microsoft", "WindowsAppSDK"),
        };

        var items = new List<Tuple<string, string, int, int>>
        {
            Tuple.Create("microsoft", "PowerToys", 24522, 1),
            Tuple.Create("microsoft", "WindowsAppSDK", 3482, 2),
        };

        var client = new GitHubClientProvider(new DeveloperIdProvider()).GetClient();
        using var tx = dataStore.Connection!.BeginTransaction();

        foreach (var repo in repositories)
        {
            var toInsert = client.Repository.Get(repo.Item1, repo.Item2).Result;
            var inserted = DataModel.Repository.GetOrCreateByOctokitRepository(dataStore, toInsert);
            Assert.AreNotEqual(DataStore.NoForeignKey, inserted.Id);
        }

        foreach (var item in items)
        {
            var toInsert = client.PullRequest.Get(item.Item1, item.Item2, item.Item3).Result;
            var inserted = DataModel.PullRequest.GetOrCreateByOctokitPullRequest(dataStore, toInsert, item.Item4);
            Assert.AreNotEqual(DataStore.NoForeignKey, inserted.Id);
        }

        tx.Commit();

        // Verify all entries were added.
        var dataStorePullRequests = dataStore.Connection.GetAll<DataModel.PullRequest>().ToList();
        Assert.AreEqual(items.Count, dataStorePullRequests.Count);

        // Verify E2E Issue object works.
        TestContext?.WriteLine($"Pull Request data from Data Object only:");
        for (var i = 1; i <= items.Count; i++)
        {
            var pullObj = DataModel.PullRequest.GetById(dataStore, i);
            Assert.IsNotNull(pullObj);
            Assert.AreNotEqual(DataStore.NoForeignKey, pullObj.Author.Id);
            Assert.AreNotEqual(DataStore.NoForeignKey, pullObj.Repository.Id);

            var index = (int)pullObj.Id - 1;
            Assert.AreEqual(items[index].Item1, pullObj.Repository.Owner.Login);
            Assert.AreEqual(items[index].Item2, pullObj.Repository.Name);
            Assert.AreEqual(items[index].Item3, pullObj.Number);

            TestContext?.WriteLine($"  Id: {pullObj.Id}  User: {pullObj.Author.Login}  Repo: {pullObj.Repository.Name} - {pullObj.Number} - {pullObj.Title}");

            // Label should have been populated with the pull request insert.
            foreach (var label in pullObj.Labels)
            {
                TestContext?.WriteLine($"      Label: {label.Name}  Color: {label.Color}");
            }

            // Assignees should have been populated with the pull request insert.
            foreach (var assignee in pullObj.Assignees)
            {
                TestContext?.WriteLine($"      Assignee: {assignee.Login}");
            }
        }

        // Validate pull request fetch from Repository object.
        TestContext?.WriteLine($"Pull Request data from Repositories:");
        for (var i = 1; i <= repositories.Count; i++)
        {
            var repoObj = DataModel.Repository.GetById(dataStore, i);
            Assert.IsNotNull(repoObj);
            var index = (int)repoObj.Id - 1;
            Assert.AreEqual(repositories[index].Item1, repoObj.Owner.Login);
            Assert.AreEqual(repositories[index].Item2, repoObj.Name);
            Assert.AreEqual(repositories[index].Item1 + '/' + repositories[index].Item2, repoObj.FullName);

            TestContext?.WriteLine($"  FullName: {repoObj.FullName}  User: {repoObj.Owner.Login}  Repo: {repoObj.Name} - {repoObj.InternalId} - {repoObj.Description}");
            foreach (var pull in repoObj.PullRequests)
            {
                TestContext?.WriteLine($"    Id: {pull.Id}  User: {pull.Author.Login} - {pull.Number} - {pull.Title}");
            }
        }
    }

    [TestMethod]
    [TestCategory("LiveData")]
    public void AddIssueFromOctokit()
    {
        using var dataStore = new DataStore("TestStore", TestHelpers.GetDataStoreFilePath(TestOptions), TestOptions.DataStoreOptions.DataStoreSchema!);
        Assert.IsNotNull(dataStore);
        dataStore.Create();
        Assert.IsNotNull(dataStore.Connection);

        // We must add repositories because Octokit Issues do not have repository populated.
        var repositories = new List<Tuple<string, string>>
        {
            Tuple.Create("microsoft", "PowerToys"),
            Tuple.Create("microsoft", "WindowsAppSDK"),
        };

        var items = new List<Tuple<string, string, int, int>>
        {
            Tuple.Create("microsoft", "PowerToys", 24692, 1),
            Tuple.Create("microsoft", "PowerToys", 24491, 1),
            Tuple.Create("microsoft", "WindowsAppSDK", 3096, 2),
            Tuple.Create("microsoft", "WindowsAppSDK", 12, 2),
        };

        var client = new GitHubClientProvider(new DeveloperIdProvider()).GetClient();
        using var tx = dataStore.Connection!.BeginTransaction();

        foreach (var repo in repositories)
        {
            var toInsert = client.Repository.Get(repo.Item1, repo.Item2).Result;
            var inserted = DataModel.Repository.GetOrCreateByOctokitRepository(dataStore, toInsert);
            Assert.AreNotEqual(DataStore.NoForeignKey, inserted.Id);
        }

        foreach (var item in items)
        {
            var toInsert = client.Issue.Get(item.Item1, item.Item2, item.Item3).Result;
            var inserted = DataModel.Issue.GetOrCreateByOctokitIssue(dataStore, toInsert, item.Item4);
            Assert.AreNotEqual(DataStore.NoForeignKey, inserted.Id);
        }

        tx.Commit();

        // Verify all entries were added.
        var dataStoreItems = dataStore.Connection.GetAll<DataModel.Issue>().ToList();
        Assert.AreEqual(items.Count, dataStoreItems.Count);

        // Verify E2E Issue object works.
        TestContext?.WriteLine($"Issue Data from Data Object only:");
        for (var i = 1; i <= items.Count; i++)
        {
            // The item should have been added in order, so validate what we specified.
            var issueObj = DataModel.Issue.GetById(dataStore, i);
            Assert.IsNotNull(issueObj);
            Assert.AreNotEqual(DataStore.NoForeignKey, issueObj.Author.Id);
            var index = (int)issueObj.Id - 1;
            Assert.AreEqual(items[index].Item3, issueObj.Number);

            TestContext?.WriteLine($"  Id: {issueObj.Id}  User: {issueObj.Author.Login}  Repo: {issueObj.Repository.Name} - {issueObj.Number} - {issueObj.Title}");

            // Label should have been populated with the issue insert.
            foreach (var label in issueObj.Labels)
            {
                TestContext?.WriteLine($"      Label: {label.Name}  Color: {label.Color}");
            }

            // Assignees should have been populated with the issue insert.
            foreach (var assignee in issueObj.Assignees)
            {
                TestContext?.WriteLine($"      Assignee: {assignee.Login}");
            }
        }

        // Validate issue fetch from Repository object.
        TestContext?.WriteLine($"Issue data from Repositories:");
        for (var i = 1; i <= repositories.Count; i++)
        {
            var repoObj = DataModel.Repository.GetById(dataStore, i);
            Assert.IsNotNull(repoObj);
            var index = (int)repoObj.Id - 1;
            Assert.AreEqual(repositories[index].Item1, repoObj.Owner.Login);
            Assert.AreEqual(repositories[index].Item2, repoObj.Name);
            Assert.AreEqual(repositories[index].Item1 + '/' + repositories[index].Item2, repoObj.FullName);

            TestContext?.WriteLine($"  FullName: {repoObj.FullName}  User: {repoObj.Owner.Login}  Repo: {repoObj.Name} - {repoObj.InternalId} - {repoObj.Description}");
            foreach (var issue in repoObj.Issues)
            {
                TestContext?.WriteLine($"    Id: {issue.Id}  User: {issue.Author.Login} - {issue.Number} - {issue.Title}");
            }
        }
    }

    [TestMethod]
    [TestCategory("LiveData")]
    public void AddPullRequestLabelFromOctokit()
    {
        using var dataStore = new DataStore("TestStore", TestHelpers.GetDataStoreFilePath(TestOptions), TestOptions.DataStoreOptions.DataStoreSchema!);
        Assert.IsNotNull(dataStore);
        dataStore.Create();
        Assert.IsNotNull(dataStore.Connection);

        var client = new GitHubClientProvider(new DeveloperIdProvider()).GetClient();

        using var tx = dataStore.Connection!.BeginTransaction();

        // Labels are on issues and pull requests and cannot be queried directly, get them from a pull.
        // Add pull so we can verify label associations
        var pull = client.PullRequest.Get("microsoft", "WindowsAppSDK", 3001).Result;
        var dataStorePull = DataModel.PullRequest.GetOrCreateByOctokitPullRequest(dataStore, pull);

        // Add all labels in the pull.
        foreach (var label in pull.Labels)
        {
            var inserted = DataModel.Label.GetOrCreateByOctokitLabel(dataStore, label);
            Assert.AreNotEqual(DataStore.NoForeignKey, inserted.Id);

            // Associate label with the pull request
            PullRequestLabel.AddLabelToPullRequest(dataStore, dataStorePull, inserted);
        }

        tx.Commit();

        // Verify retrieval and input into data objects.
        var dataStoreItems = dataStore.Connection.GetAll<DataModel.Label>().ToList();
        Assert.AreEqual(pull.Labels.Count, dataStoreItems.Count);
        TestContext?.WriteLine("Labels from DB Label Table");
        foreach (var label in dataStoreItems)
        {
            TestContext?.WriteLine($"  Id: {label.Id}  InternalId: {label.InternalId}  Name: {label.Name}  Color: {label.Color}  Default: {label.IsDefault}  Desc: {label.Description}");

            // Ids are inserted in order starting at 1, so they should match the list index - 1.
            var index = (int)label.Id - 1;
            Assert.AreEqual(pull.Labels[index].Id, label.InternalId);
            Assert.AreEqual(pull.Labels[index].Name, label.Name);
        }

        // Verify the mapping of Pull Request labels to pull request works.
        var pullFromDataStore = DataModel.PullRequest.GetById(dataStore, dataStorePull.Id);
        Assert.IsNotNull(pullFromDataStore);
        var pullRequestLabels = pullFromDataStore.Labels;
        Assert.AreEqual(pull.Labels.Count, pullRequestLabels.Count());
        TestContext?.WriteLine("Labels from PullRequest.Labels");
        foreach (var label in pullRequestLabels)
        {
            TestContext?.WriteLine($"Name: {label.Name}  Color: {label.Color}");
        }
    }

    [TestMethod]
    [TestCategory("LiveData")]
    public void AddIssueLabelFromOctokit()
    {
        using var dataStore = new DataStore("TestStore", TestHelpers.GetDataStoreFilePath(TestOptions), TestOptions.DataStoreOptions.DataStoreSchema!);
        Assert.IsNotNull(dataStore);
        dataStore.Create();
        Assert.IsNotNull(dataStore.Connection);

        var client = new GitHubClientProvider(new DeveloperIdProvider()).GetClient();

        using var tx = dataStore.Connection!.BeginTransaction();

        // Labels are on issues and pull requests and cannot be queried directly, get them from a pull.
        // Add pull so we can verify label associations.
        var issue = client.Issue.Get("microsoft", "WindowsAppSDK", 3096).Result;
        var dataStoreIssue = DataModel.Issue.GetOrCreateByOctokitIssue(dataStore, issue);

        // Add all labels in the pull.
        foreach (var label in issue.Labels)
        {
            var inserted = DataModel.Label.GetOrCreateByOctokitLabel(dataStore, label);
            Assert.AreNotEqual(DataStore.NoForeignKey, inserted.Id);

            // Associate label with the pull request.
            IssueLabel.AddLabelToIssue(dataStore, dataStoreIssue, inserted);
        }

        tx.Commit();

        // Verify retrieval and input into data objects.
        var dataStoreItems = dataStore.Connection.GetAll<DataModel.Label>().ToList();
        Assert.AreEqual(issue.Labels.Count, dataStoreItems.Count);
        foreach (var label in dataStoreItems)
        {
            TestContext?.WriteLine($"  Id: {label.Id}  InternalId: {label.InternalId}  Name: {label.Name}  Color: {label.Color}  Default: {label.IsDefault}  Desc: {label.Description}");

            // Ids are inserted in order starting at 1, so they should match the list index - 1.
            var index = (int)label.Id - 1;
            Assert.AreEqual(issue.Labels[index].Id, label.InternalId);
            Assert.AreEqual(issue.Labels[index].Name, label.Name);
        }

        // Verify the mapping of Pull Request labels to pull request works.
        var issueFromDataStore = DataModel.Issue.GetById(dataStore, dataStoreIssue.Id);
        Assert.IsNotNull(issueFromDataStore);
        var issueLabels = issueFromDataStore.Labels;
        Assert.AreEqual(issue.Labels.Count, issueLabels.Count());
        TestContext?.WriteLine("Labels from GetLabelsForIssue: ");
        foreach (var label in issueLabels)
        {
            TestContext?.WriteLine($"Name:  {label.Name}   Color:  {label.Color}");
        }
    }
}
