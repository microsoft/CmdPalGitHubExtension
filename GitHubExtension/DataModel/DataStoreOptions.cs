// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace GitHubExtension.DataModel;

public partial class DataStoreOptions
{
    private const string DataStoreFileNameDefault = "GitHubData.db";

    public string DataStoreFileName { get; set; } = DataStoreFileNameDefault;

    // The Temp Path is used for storage by default so tests can run this code without being packaged.
    // If we directly put in the ApplicationData folder, it would fail anytime the program was not packaged.
    // For use with packaged application, set in Options to:
    //     ApplicationData.Current.LocalFolder.Path
    private readonly string _dataStoreFolderPathDefault = Path.Combine(Path.GetTempPath(), "GitHubExtension");

    // ApplicationData is not static, using a static folder for initialization.
    private string? _dataStoreFolderPath;

    public string DataStoreFolderPath
    {
        get => _dataStoreFolderPath is null ? _dataStoreFolderPathDefault : _dataStoreFolderPath;
        set => _dataStoreFolderPath = string.IsNullOrEmpty(value) ? _dataStoreFolderPathDefault : value;
    }

    public IDataStoreSchema? DataStoreSchema { get; set; }
}
