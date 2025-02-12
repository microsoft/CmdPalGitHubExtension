// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataModel;
using GitHubExtension.DeveloperId;
using Serilog;
using Windows.Storage;

namespace GitHubExtension.PersistentData;

public class PersistentDataManager : IDisposable
{
    private const string DataStoreFileName = "PersistentGitHubData.db";

    private DataStore DataStore { get; set; }

    public DataStoreOptions DataStoreOpions { get; set; }

    private void ValidadeDataStore()
    {
        if (DataStore == null || !DataStore.IsConnected)
        {
            throw new DataStoreInaccessibleException("DataStore is not available.");
        }
    }

    private void ValidateRepositoryOwnerAndName(string owner, string repositoryName)
    {
        if (string.IsNullOrEmpty(owner))
        {
            throw new ArgumentNullException(nameof(owner));
        }

        if (string.IsNullOrEmpty(repositoryName))
        {
            throw new ArgumentNullException(nameof(repositoryName));
        }
    }

    private static readonly Lazy<DataStoreOptions> _lazyDataStoreOptions = new(DefaultOptionsInit);

    private static DataStoreOptions DefaultOptions => _lazyDataStoreOptions.Value;

    private static DataStoreOptions DefaultOptionsInit()
    {
        return new DataStoreOptions
        {
            DataStoreFolderPath = ApplicationData.Current.LocalFolder.Path,
            DataStoreSchema = new PersistentDataSchema(),
        };
    }

    public static PersistentDataManager? CreateInstance(DataStoreOptions? options = null)
    {
        options ??= DefaultOptions;

        try
        {
            return new PersistentDataManager(options);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating PersistentDataManager: {ex.Message}");
            return null;
        }
    }

    public PersistentDataManager(DataStoreOptions dataStoreOptions)
    {
        if (dataStoreOptions.DataStoreSchema is null)
        {
            throw new ArgumentNullException(nameof(dataStoreOptions), "DataStoreSchema cannot be null.");
        }

        DataStoreOpions = dataStoreOptions;

        DataStore = new DataStore(
            "PersistentDataStore",
            Path.Combine(dataStoreOptions.DataStoreFolderPath, DataStoreFileName),
            dataStoreOptions.DataStoreSchema);

        DataStore.Create();
    }

    private bool _disposed;

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            DataStore?.Dispose();
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private async Task ValidateRepository(string owner, string name)
    {
        ValidateRepositoryOwnerAndName(owner, name);
        Octokit.GitHubClient? client = DeveloperIdProvider.GetInstance().GetLoggedInDeveloperIdsInternal().First().GitHubClient;
        _ = await client.Repository.Get(owner, name);
    }

    // Management code goes here
    public async Task AddRepositoryAsync(string owner, string name, Octokit.GitHubClient? client = null)
    {
        await ValidateRepository(owner, name);
        ValidadeDataStore();
        Log.Information($"Adding repository {owner}/{name}.");

        if (Repository.Get(DataStore, owner, name) != null)
        {
            throw new InvalidOperationException($"Repository {owner}/{name} already exists.");
        }

        Repository.Add(DataStore, owner, name);
    }

    public async void RemoveRepositoryAsync(string owner, string name)
    {
        await Task.Run(() =>
        {
            // No need to validate repository here as it was already
            // validated when added.
            ValidadeDataStore();
            Log.Information($"Removing repository {owner}/{name}.");
            Repository.Remove(DataStore, owner, name);
        });
    }

    public async Task<IEnumerable<Repository>> GetAllRepositoriesAsync()
    {
        return await Task.Run(() =>
        {
            ValidadeDataStore();
            return Repository.GetAll(DataStore);
        });
    }
}
