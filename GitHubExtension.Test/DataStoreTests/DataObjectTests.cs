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
    public void DateTimeExtension()
    {
        var now = DateTime.Now;
        TestContext?.WriteLine($"Now: {now}");
        var nowAsInteger = now.ToDataStoreInteger();
        TestContext?.WriteLine($"NowAsDataStoreInteger: {nowAsInteger}");
        var nowFromInteger = nowAsInteger.ToDateTime();
        TestContext?.WriteLine($"NowFromDataStoreInteger: {nowFromInteger}");

        // We should not lose precision in the conversion to/from datastore format.
        Assert.AreEqual(now, nowFromInteger);
        Assert.AreEqual(now, now.ToDataStoreInteger().ToDateTime());
        Assert.AreEqual(now, now.ToDataStoreString().ToDateTime());

        // Working with the value should be as easy as working with dates, converting to numbers,
        // and using them in queries.
        var thirtyDays = new TimeSpan(30, 0, 0);
        TestContext?.WriteLine($"ThirtyDays: {thirtyDays}");
        var thirtyDaysAgo = now.Subtract(thirtyDays);
        TestContext?.WriteLine($"ThirtyDaysAgo: {thirtyDaysAgo}");
        var thirtyDaysAgoAsInteger = thirtyDaysAgo.ToDataStoreInteger();
        TestContext?.WriteLine($"ThirtyDaysAgoAsInteger: {thirtyDaysAgoAsInteger}");
        TestContext?.WriteLine($"ThirtyDays Ticks: {thirtyDays.Ticks}");
        TestContext?.WriteLine($"IntegerDiff: {nowAsInteger - thirtyDaysAgoAsInteger}");

        // Doing some timespan manipulation should still result in the same tick difference.
        // Also verify TimeSpan converters.
        Assert.AreEqual(thirtyDays.Ticks, nowAsInteger - thirtyDaysAgoAsInteger);
        Assert.AreEqual(thirtyDays, thirtyDays.ToDataStoreInteger().ToTimeSpan());
        Assert.AreEqual(thirtyDays, thirtyDays.ToDataStoreString().ToTimeSpan());

        // Test adding metadata time as string to the datastore.
        using var dataStore = new DataStore("TestStore", TestHelpers.GetDataStoreFilePath(TestOptions), TestOptions.DataStoreOptions.DataStoreSchema!);
        Assert.IsNotNull(dataStore);
        dataStore.Create();
        Assert.IsNotNull(dataStore.Connection);
        MetaData.AddOrUpdate(dataStore, "Now", now.ToDataStoreString());
        MetaData.AddOrUpdate(dataStore, "ThirtyDays", thirtyDays.ToDataStoreString());
        var nowFromMetaData = MetaData.Get(dataStore, "Now");
        Assert.IsNotNull(nowFromMetaData);
        var thirtyDaysFromMetaData = MetaData.Get(dataStore, "ThirtyDays");
        Assert.IsNotNull(thirtyDaysFromMetaData);
        Assert.AreEqual(now, nowFromMetaData.ToDateTime());
        Assert.AreEqual(thirtyDays, thirtyDaysFromMetaData.ToTimeSpan());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ReadAndWriteMetaData()
    {
        using var dataStore = new DataStore("TestStore", TestHelpers.GetDataStoreFilePath(TestOptions), TestOptions.DataStoreOptions.DataStoreSchema!);
        Assert.IsNotNull(dataStore);
        dataStore.Create();
        Assert.IsNotNull(dataStore.Connection);

        var metadata = new List<MetaData>
        {
            { new MetaData { Key = "Kittens", Value = "Cute" } },
            { new MetaData { Key = "Puppies", Value = "LotsOfWork" } },
        };

        using var tx = dataStore.Connection!.BeginTransaction();
        dataStore.Connection.Insert(metadata[0]);
        dataStore.Connection.Insert(metadata[1]);
        tx.Commit();

        // Verify retrieval and input into data objects.
        var dataStoreMetaData = dataStore.Connection.GetAll<MetaData>().ToList();
        Assert.AreEqual(2, dataStoreMetaData.Count);
        foreach (var metaData in dataStoreMetaData)
        {
            TestContext?.WriteLine($"  Id: {metaData.Id}  Key: {metaData.Key}  Value: {metaData.Value}");

            Assert.IsTrue(metaData.Id == 1 || metaData.Id == 2);

            if (metaData.Id == 1)
            {
                Assert.AreEqual("Kittens", metaData.Key);
                Assert.AreEqual("Cute", metaData.Value);
            }

            if (metaData.Id == 2)
            {
                Assert.AreEqual("Puppies", metaData.Key);
                Assert.AreEqual("LotsOfWork", metaData.Value);
            }
        }

        // Verify direct add and retrieval.
        MetaData.AddOrUpdate(dataStore, "Puppies", "WorthIt!");
        MetaData.AddOrUpdate(dataStore, "Spiders", "Nope");
        Assert.AreEqual("Cute", MetaData.Get(dataStore, "Kittens"));
        Assert.AreEqual("WorthIt!", MetaData.Get(dataStore, "Puppies"));
        Assert.AreEqual("Nope", MetaData.Get(dataStore, "Spiders"));
        dataStoreMetaData = dataStore.Connection.GetAll<MetaData>().ToList();
        foreach (var metaData in dataStoreMetaData)
        {
            TestContext?.WriteLine($"  Id: {metaData.Id}  Key: {metaData.Key}  Value: {metaData.Value}");
        }
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ReadAndWriteUser()
    {
        using var dataStore = new DataStore("TestStore", TestHelpers.GetDataStoreFilePath(TestOptions), TestOptions.DataStoreOptions.DataStoreSchema!);
        Assert.IsNotNull(dataStore);
        dataStore.Create();
        Assert.IsNotNull(dataStore.Connection);

        var users = new List<User>
        {
            { new User { Login = "Kittens", InternalId = 16, AvatarUrl = "https://www.microsoft.com", Type = "Cat" } },
            { new User { Login = "Liberty", InternalId = 7, AvatarUrl = "https://www.microsoft.com", Type = "Dog" } },
        };

        using var tx = dataStore.Connection!.BeginTransaction();
        dataStore.Connection.Insert(users[0]);
        dataStore.Connection.Insert(users[1]);
        tx.Commit();

        // Verify retrieval and input into data objects.
        var dataStoreUsers = dataStore.Connection.GetAll<User>().ToList();
        Assert.AreEqual(2, dataStoreUsers.Count);
        foreach (var user in dataStoreUsers)
        {
            TestContext?.WriteLine($"  User: {user.Id}: {user.Login} - {user.InternalId} - {user.AvatarUrl} - {user.Type}");

            Assert.IsTrue(user.Id == 1 || user.Id == 2);

            if (user.Id == 1)
            {
                Assert.AreEqual("Kittens", user.Login);
                Assert.AreEqual(16, user.InternalId);
                Assert.AreEqual("Cat", user.Type);
                Assert.AreEqual("https://www.microsoft.com", user.AvatarUrl);
            }

            if (user.Id == 2)
            {
                Assert.AreEqual("Liberty", user.Login);
                Assert.AreEqual(7, user.InternalId);
                Assert.AreEqual("Dog", user.Type);
                Assert.AreEqual("https://www.microsoft.com", user.AvatarUrl);
            }
        }
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ReadAndWriteRepository()
    {
        using var dataStore = new DataStore("TestStore", TestHelpers.GetDataStoreFilePath(TestOptions), TestOptions.DataStoreOptions.DataStoreSchema!);
        Assert.IsNotNull(dataStore);
        dataStore.Create();
        Assert.IsNotNull(dataStore.Connection);

        using var tx = dataStore.Connection.BeginTransaction();

        // Add User record
        dataStore.Connection.Insert(new User { Login = "Kittens", InternalId = 16, AvatarUrl = "https://www.microsoft.com", Type = "Cat" });

        // Add repositories
        var repositories = new List<Repository>
        {
            { new Repository { OwnerId = 1, InternalId = 47, Name = "TestRepo1", Description = "Short Desc", HtmlUrl = "https://www.microsoft.com", DefaultBranch = "main" } },
            { new Repository { OwnerId = 1, InternalId = 117, Name = "TestRepo2", Description = "Short Desc", HtmlUrl = "https://www.microsoft.com", DefaultBranch = "main" } },
        };
        dataStore.Connection.Insert(repositories[0]);
        dataStore.Connection.Insert(repositories[1]);
        tx.Commit();

        // Verify retrieval and input into data objects.
        var dataStoreRepositories = dataStore.Connection.GetAll<Repository>().ToList();
        Assert.AreEqual(2, dataStoreRepositories.Count);
        foreach (var repo in dataStoreRepositories)
        {
            // Get User for the repo
            var user = dataStore.Connection.Get<User>(repo.OwnerId);
            TestContext?.WriteLine($"  User: {user.Login}  Repo: {repo.Name} - {repo.InternalId} - {repo.Description}");
            Assert.AreEqual("Kittens", user.Login);
            Assert.IsTrue(repo.Id == 1 || repo.Id == 2);

            if (repo.Id == 1)
            {
                Assert.AreEqual("Kittens", user.Login);
                Assert.AreEqual(47, repo.InternalId);
                Assert.AreEqual("TestRepo1", repo.Name);
                Assert.AreEqual("https://www.microsoft.com", repo.HtmlUrl);
            }

            if (repo.Id == 2)
            {
                Assert.AreEqual("Kittens", user.Login);
                Assert.AreEqual(117, repo.InternalId);
                Assert.AreEqual("TestRepo2", repo.Name);
                Assert.AreEqual("https://www.microsoft.com", repo.HtmlUrl);
            }
        }
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ReadAndWriteIssue()
    {
        using var dataStore = new DataStore("TestStore", TestHelpers.GetDataStoreFilePath(TestOptions), TestOptions.DataStoreOptions.DataStoreSchema!);
        Assert.IsNotNull(dataStore);
        dataStore.Create();
        Assert.IsNotNull(dataStore.Connection);

        using var tx = dataStore.Connection!.BeginTransaction();

        // Add User record
        dataStore.Connection.Insert(new User { Login = "Kittens", InternalId = 16, AvatarUrl = "https://www.microsoft.com", Type = "Cat" });

        // Add repository record
        dataStore.Connection.Insert(new Repository { OwnerId = 1, InternalId = 47, Name = "TestRepo1", Description = "Short Desc", HtmlUrl = "https://www.microsoft.com", DefaultBranch = "main", HasIssues = 1 });

        var issues = new List<Issue>
        {
            { new Issue { AuthorId = 1, Number = 1111, InternalId = 18, Title = "No worky", Body = "This feature doesn't work.", HtmlUrl = "https://www.microsoft.com", RepositoryId = 1 } },
            { new Issue { AuthorId = 1, Number = 47, InternalId = 20, Title = "Missing Tests", Body = "More tests needed.", HtmlUrl = "https://www.microsoft.com", RepositoryId = 1 } },
        };
        dataStore.Connection.Insert(issues[0]);
        dataStore.Connection.Insert(issues[1]);
        tx.Commit();

        // Verify retrieval and input into data objects.
        var dataStoreIssues = dataStore.Connection.GetAll<Issue>().ToList();
        Assert.AreEqual(2, dataStoreIssues.Count);
        foreach (var issue in dataStoreIssues)
        {
            // Get User  and Repo info
            var user = dataStore.Connection.Get<User>(issue.AuthorId);
            var repo = dataStore.Connection.Get<Repository>(issue.RepositoryId);

            TestContext?.WriteLine($"  User: {user.Login}  Repo: {repo.Name} - {issue.Number} - {issue.Title}");
            Assert.AreEqual("Kittens", user.Login);
            Assert.AreEqual("TestRepo1", repo.Name);
            Assert.IsTrue(issue.Id == 1 || issue.Id == 2);

            if (issue.Id == 1)
            {
                Assert.AreEqual("Kittens", user.Login);
                Assert.AreEqual(1111, issue.Number);
                Assert.AreEqual("TestRepo1", repo.Name);
                Assert.AreEqual("No worky", issue.Title);
            }

            if (issue.Id == 2)
            {
                Assert.AreEqual("Kittens", user.Login);
                Assert.AreEqual(47, issue.Number);
                Assert.AreEqual("TestRepo1", repo.Name);
                Assert.AreEqual("Missing Tests", issue.Title);
            }
        }
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ReadAndWritePullRequest()
    {
        using var dataStore = new DataStore("TestStore", TestHelpers.GetDataStoreFilePath(TestOptions), TestOptions.DataStoreOptions.DataStoreSchema!);
        Assert.IsNotNull(dataStore);
        dataStore.Create();
        Assert.IsNotNull(dataStore.Connection);

        using var tx = dataStore.Connection!.BeginTransaction();

        // Add User record
        dataStore.Connection.Insert(new User { Login = "Kittens", InternalId = 16, AvatarUrl = "https://www.microsoft.com", Type = "Cat" });

        // Add repository record
        dataStore.Connection.Insert(new Repository { OwnerId = 1, InternalId = 47, Name = "TestRepo1", Description = "Short Desc", HtmlUrl = "https://www.microsoft.com", DefaultBranch = "main", HasIssues = 1 });

        var prs = new List<PullRequest>
        {
            { new PullRequest { AuthorId = 1, Number = 12, InternalId = 4, Title = "Fix no worky", Body = "This feature doesn't work.", HtmlUrl = "https://www.microsoft.com", RepositoryId = 1 } },
            { new PullRequest { AuthorId = 1, Number = 85, InternalId = 22, Title = "Implement Tests", Body = "More tests needed.", HtmlUrl = "https://www.microsoft.com", RepositoryId = 1 } },
        };
        dataStore.Connection.Insert(prs[0]);
        dataStore.Connection.Insert(prs[1]);
        tx.Commit();

        // Verify retrieval and input into data objects.
        var dataStorePulls = dataStore.Connection.GetAll<PullRequest>().ToList();
        Assert.AreEqual(2, dataStorePulls.Count);
        foreach (var pull in dataStorePulls)
        {
            // Get User  and Repo info
            var user = dataStore.Connection.Get<User>(pull.AuthorId);
            var repo = dataStore.Connection.Get<Repository>(pull.RepositoryId);

            TestContext?.WriteLine($"  User: {user.Login}  Repo: {repo.Name} - {pull.Number} - {pull.Title}");
            Assert.AreEqual("Kittens", user.Login);
            Assert.AreEqual("TestRepo1", repo.Name);
            Assert.IsTrue(pull.Id == 1 || pull.Id == 2);

            if (pull.Id == 1)
            {
                Assert.AreEqual("Kittens", user.Login);
                Assert.AreEqual(12, pull.Number);
                Assert.AreEqual("TestRepo1", repo.Name);
                Assert.AreEqual("Fix no worky", pull.Title);
            }

            if (pull.Id == 2)
            {
                Assert.AreEqual("Kittens", user.Login);
                Assert.AreEqual(85, pull.Number);
                Assert.AreEqual("TestRepo1", repo.Name);
                Assert.AreEqual("Implement Tests", pull.Title);
            }
        }
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ReadAndWriteSearch()
    {
        using var dataStore = new DataStore("TestStore", TestHelpers.GetDataStoreFilePath(TestOptions), TestOptions.DataStoreOptions.DataStoreSchema!);
        Assert.IsNotNull(dataStore);
        dataStore.Create();
        Assert.IsNotNull(dataStore.Connection);

        using var tx = dataStore.Connection!.BeginTransaction();

        var searches = new List<Search>
        {
            { new Search { Name = "Test 0", SearchString = "is:issue lets hope" } },
            { new Search { Name = "Test 1", SearchString = "is:pr for the best" } },
        };

        dataStore.Connection.Insert(searches[0]);
        dataStore.Connection.Insert(searches[1]);

        tx.Commit();

        // Verify retrieval and input into data objects.
        var dataStoreSearches = dataStore.Connection.GetAll<Search>().ToList();
        Assert.AreEqual(2, dataStoreSearches.Count);

        foreach (var search in dataStoreSearches)
        {
            TestContext?.WriteLine($"  Search: {search.Name} - {search.SearchString}");
            Assert.IsTrue(search.Id == 1 || search.Id == 2);
            if (search.Id == 1)
            {
                Assert.AreEqual("Test 0", search.Name);
                Assert.AreEqual("is:issue lets hope", search.SearchString);
            }

            if (search.Id == 2)
            {
                Assert.AreEqual("Test 1", search.Name);
                Assert.AreEqual("is:pr for the best", search.SearchString);
            }
        }
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ResetDataStore()
    {
        using var dataStore = new DataStore("TestStore", TestHelpers.GetDataStoreFilePath(TestOptions), TestOptions.DataStoreOptions.DataStoreSchema!);
        Assert.IsNotNull(dataStore);
        dataStore.Create();
        using var tx = dataStore.Connection!.BeginTransaction();

        var searches = new List<Search>
        {
            { new Search { Name = "Test 0", SearchString = "is:issue lets hope" } },
            { new Search { Name = "Test 1", SearchString = "is:pr for the best" } },
        };

        dataStore.Connection.Insert(searches[0]);
        dataStore.Connection.Insert(searches[1]);

        tx.Commit();

        // Verify retrieval and input into data objects.
        var dataStoreSearches = dataStore.Connection.GetAll<Search>().ToList();
        Assert.AreEqual(2, dataStoreSearches.Count);

        dataStore.Reset();

        var dataStoreSearches2 = dataStore.Connection.GetAll<Search>().ToList();
        Assert.AreEqual(0, dataStoreSearches2.Count);
    }
}
