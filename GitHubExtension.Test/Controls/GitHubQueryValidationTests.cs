// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.Controls.Forms;
using GitHubExtension.DataModel;
using GitHubExtension.Helpers;
using GitHubExtension.PersistentData;
using GitHubExtension.Test.Helpers;
using GitHubExtension.Test.PersistentData;
using Moq;

namespace GitHubExtension.Test.Controls;

[TestClass]
public class GitHubQueryValidationTests
{
    private (PersistentDataManager PersistentDataManager, IResources Resources, SavedSearchesMediator Mediator, DataStoreOptions DataStoreOptions) CreateTestContext()
    {
        var dataStoreOptions = PersistentDataManagerTestsSetup.GetDataStoreOptions();
        var persistentDataManager = TestHelpers.CreatePersistentDataManager(dataStoreOptions);
        var resources = new Mock<IResources>().Object;
        var mediator = new SavedSearchesMediator();
        return (persistentDataManager, resources, mediator, dataStoreOptions);
    }

    [DataRow("is:open", "Test Search")]
    [DataRow("is:issue", "Test Search")]
    [DataRow("is:pr", "Test Search")]
    [DataRow("is:open is:issue", "Test Search")]
    [DataRow("is:issue repo:microsoft/PowerToys", "Test Search")]
    [DataRow("is:pr author:octocat", "Test Search")]
    [DataRow("is:issue state:open label:\\\"Product-Command Palette\\\"", "test", "is:issue state:open label:\"Product-Command Palette\"")]
    [DataRow("involves:defunkt language:javascript", "Test Search")]
    [DataRow("org:github created:>2022-01-01", "Test Search")]
    [DataRow("is:issue assignee:@me milestone:v1.0", "Test Search")]
    [DataRow("is:pr review:approved", "Test Search")]
    [DataRow("is:issue in:title error", "Test Search")]
    [DataRow("is:pr merged:>=2023-01-01", "Test Search")]
    [DataRow("label:enhancement", "Test Search")]
    [DataRow("label:bug label:help-wanted label:documentation", "Test Search")]
    [DataRow("-label:wontfix", "Test Search")]
    [DataRow("-label:wontfix -label:duplicate -label:invalid", "Test Search")]
    [DataRow("is:issue -is:closed -author:bot", "Test Search")]
    [DataRow("is:pr label:enhancement -label:wontfix repo:microsoft/PowerToys -is:draft", "Test Search")]
    [DataRow("is:open AND (is:issue OR is:pr) NOT author:bot devhome", "Test Search")]
    [DataRow("repo:microsoft/terminal repo:microsoft/PowerToys repo:microsoft/vscode is:open is:issue", "Test Search")]
    [TestMethod]
    public async Task ValidateSearch_SavesExpectedSearchString(string testSearchString, string searchName, string? expectedSearchStringIfDifferentFormatThanOriginal = null)
    {
        var (persistentDataManager, resources, mediator, dataStoreOptions) = CreateTestContext();
        try
        {
            var saveSearchForm = new SaveSearchForm(persistentDataManager, resources, mediator);
            var payload = TestHelpers.CreateJsonPayload(testSearchString, searchName, false);

            var tcs = TestHelpers.CreateTaskCompletionSource(mediator);
            saveSearchForm.SubmitForm(payload, string.Empty);
            await tcs.Task;

            var searches = await persistentDataManager.GetSavedSearches();
            Assert.IsTrue(searches.Count() == 1);
            Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, expectedSearchStringIfDifferentFormatThanOriginal ?? testSearchString, StringComparison.OrdinalIgnoreCase)));
        }
        finally
        {
            persistentDataManager.Dispose();
            PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
        }
    }

    [DataRow("https://github.com/microsoft/PowerToys/issues?q=is:open+label:bug", "repo:microsoft/PowerToys is:open label:bug", "Test Search")]
    [DataRow("https://github.com/microsoft/PowerToys/issues", "repo:microsoft/PowerToys is:issue is:open", "Test Search")]
    [DataRow("https://github.com/microsoft/PowerToys/issues?q=is:issue+is:closed", "repo:microsoft/PowerToys is:issue is:closed", "Test Search")]
    [DataRow("https://github.com/microsoft/PowerToys/pulls", "repo:microsoft/PowerToys is:pr is:open", "Test Search")]
    [DataRow("https://github.com/search/issues", "is:issue", "Test Search")]
    [DataRow("not a url", "not a url", "Test Search")]
    [DataRow("   ", "   ", "Test Search")]
    [DataRow("https://github.com/search?q=repo:microsoft/PowerToys+is:open+is:issue+label:bug+author:octocat", "repo:microsoft/PowerToys is:open is:issue label:bug author:octocat", "Test Search")]
    [DataRow("https://github.com/search?q=repo:microsoft/PowerToys+is:open+-label:wontfix", "repo:microsoft/PowerToys is:open -label:wontfix", "Test Search")]
    [DataRow("https://github.com/search?q=repo:microsoft/terminal+repo:microsoft/PowerToys+repo:microsoft/vscode+is:open+is:issue", "repo:microsoft/terminal repo:microsoft/PowerToys repo:microsoft/vscode is:open is:issue", "Test Search")]
    [DataRow("https://github.com/search?q=repo:microsoft/PowerToys+state:open+state:closed", "repo:microsoft/PowerToys state:open state:closed", "Test Search")]
    [DataRow("https://github.com/search?q=repo:microsoft/PowerToys+sort:updated-desc+sort:created-asc", "repo:microsoft/PowerToys sort:updated-desc sort:created-asc", "Test Search")]
    [DataRow("https://github.com/search?q=repo:microsoft/PowerToys+language:csharp+language:javascript+milestone:v1.0+milestone:v2.0+created:>2022-01-01+updated:<2023-01-01", "repo:microsoft/PowerToys language:csharp language:javascript milestone:v1.0 milestone:v2.0 created:>2022-01-01 updated:<2023-01-01", "Test Search")]
    [TestMethod]
    public async Task ValidateURL_ParsesAndSavesExpectedSearchString(string url, string expected, string searchName)
    {
        var (persistentDataManager, resources, mediator, dataStoreOptions) = CreateTestContext();
        try
        {
            var saveSearchForm = new SaveSearchForm(persistentDataManager, resources, mediator);
            var payload = TestHelpers.CreateJsonPayload(url, searchName, false);

            var tcs = TestHelpers.CreateTaskCompletionSource(mediator);
            saveSearchForm.SubmitForm(payload, string.Empty);
            await tcs.Task;

            var searches = await persistentDataManager.GetSavedSearches();
            Assert.IsTrue(searches.Count() == 1);
            Assert.IsTrue(searches.Any(s => string.Equals(s.SearchString, expected, StringComparison.OrdinalIgnoreCase) && string.Equals(s.Name, searchName, StringComparison.OrdinalIgnoreCase)));
        }
        finally
        {
            persistentDataManager.Dispose();
            PersistentDataManagerTestsSetup.Cleanup(dataStoreOptions.DataStoreFolderPath);
        }
    }
}
