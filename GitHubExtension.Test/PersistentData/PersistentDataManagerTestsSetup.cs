// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataModel;
using GitHubExtension.PersistentData;

namespace GitHubExtension.Test.PersistentData;

public partial class PersistentDataManagerTests
{
    public DataStoreOptions GetDataStoreOptions()
    {
        var guid = Guid.NewGuid();
        return new()
        {
            // Keep path name short to avoid path length issues.
            DataStoreFolderPath = Path.Combine(Path.GetTempPath(), $"PDGH-{guid}"),
            DataStoreFileName = "PersistentGitHubData-Test.db",
            DataStoreSchema = new PersistentDataSchema(),
        };
    }

    public void Cleanup(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }
}
