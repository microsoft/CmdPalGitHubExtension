// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Test.Helpers;

namespace GitHubExtension.Test.DataStore;

[TestClass]
public partial class DataStoreTests
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    private TestOptions _testOptions = new();

    private TestOptions TestOptions
    {
        get => _testOptions;
        set => _testOptions = value;
    }

    [TestInitialize]
    public void TestInitialize()
    {
        TestOptions = TestHelpers.SetupTempTestOptions(TestContext!);
        TestHelpers.ConfigureTestLog(TestOptions, TestContext!);
    }

    [TestCleanup]
    public void Cleanup()
    {
        TestHelpers.CloseTestLog();
        TestHelpers.CleanupTempTestOptions(TestOptions, TestContext!);
    }
}
