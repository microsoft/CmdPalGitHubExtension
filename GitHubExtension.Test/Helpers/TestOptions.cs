// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataModel;

namespace GitHubExtension.Test;

public partial class TestOptions
{
    public string LogFileFolderRoot { get; set; } = string.Empty;

    public string LogFileFolderName { get; set; } = "{now}";

    public string LogFileName { get; set; } = string.Empty;

    public string LogFileFolderPath => Path.Combine(LogFileFolderRoot, LogFileFolderName);

    public DataStoreOptions DataStoreOptions { get; set; }

    public TestOptions()
    {
        DataStoreOptions = new DataStoreOptions();
    }
}
