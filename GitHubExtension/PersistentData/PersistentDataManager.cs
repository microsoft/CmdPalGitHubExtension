// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.DataManager.GitHubDataManager;
using GitHubExtension.DataModel;
using GitHubExtension.DataModel.Enums;
using GitHubExtension.DeveloperId;
using Octokit;
using Serilog;
using Windows.Storage;

namespace GitHubExtension.PersistentData;

public class PersistentDataManager : IDisposable, ISearchRepository
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", nameof(PersistentDataManager)));

    private static readonly ILogger _log = _logger.Value;

    private const string DataStoreFileName = "PersistentGitHubData.db";

    private readonly IDeveloperIdProvider _developerIdProvider;

    private DataStore DataStore { get; set; }

    public DataStoreOptions DataStoreOpions { get; set; }

    private void ValidateDataStore()
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

    public PersistentDataManager(IDeveloperIdProvider developerIdProvider, DataStoreOptions? dataStoreOptions = null)
    {
        _developerIdProvider = developerIdProvider;

        dataStoreOptions ??= DefaultOptions;

        if (dataStoreOptions.DataStoreSchema is null)
        {
            throw new ArgumentNullException(nameof(dataStoreOptions), "DataStoreSchema cannot be null.");
        }

        DataStoreOpions = dataStoreOptions;

        DataStore = new DataStore(
            "PersistentDataStore",
            Path.Combine(dataStoreOptions.DataStoreFolderPath, DataStoreFileName),
            dataStoreOptions.DataStoreSchema);

        DataStore.Create(false);
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

    // Repository methods
    private async Task ValidateRepository(string owner, string name)
    {
        ValidateRepositoryOwnerAndName(owner, name);
        GitHubClient? client = _developerIdProvider.GetLoggedInDeveloperIdsInternal().First().GitHubClient;
        _ = await client.Repository.Get(owner, name);
    }

    // Management code goes here
    public async Task AddRepositoryAsync(string owner, string name, GitHubClient? client = null)
    {
        await ValidateRepository(owner, name);
        ValidateDataStore();
        _log.Information($"Adding repository {owner}/{name}.");

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
            ValidateDataStore();
            _log.Information($"Removing repository {owner}/{name}.");
            Repository.Remove(DataStore, owner, name);
        });
    }

    public async Task<IEnumerable<Repository>> GetAllRepositoriesAsync()
    {
        return await Task.Run(() =>
        {
            ValidateDataStore();
            return Repository.GetAll(DataStore);
        });
    }

    // Search methods
    private async Task ValidateSearch(string searchString, SearchType searchType)
    {
        // TODO: Change this request depending on the search type in cade we add Repositories.
        GitHubClient? client = _developerIdProvider.GetLoggedInDeveloperIdsInternal().First().GitHubClient;
        var issuesOptions = new SearchIssuesRequest(searchString)
        {
            State = ItemState.Open,
            Type = searchType == SearchType.Issues ? IssueTypeQualifier.Issue : IssueTypeQualifier.PullRequest,
            SortField = IssueSearchSort.Updated,
            Order = SortDirection.Descending,
        };

        _ = await client.Search.SearchIssues(issuesOptions);
    }

    private async Task AddSearchAsync(string name, string searchString, SearchType searchType, Octokit.GitHubClient? client = null)
    {
        await ValidateSearch(searchString, searchType);
        ValidateDataStore();

        _log.Information($"Adding search: {name} - {searchString} - {searchType}.");
        if (Search.Get(DataStore, name, searchString) != null)
        {
            throw new InvalidOperationException($"Search {name} - {searchString} - {searchType} already exists.");
        }

        Search.Add(DataStore, name, searchString);
    }

    private async Task RemoveSearchAsync(string name, string searchString, SearchType searchType)
    {
        await Task.Run(() =>
        {
            ValidateDataStore();
            _log.Information($"Removing search: {name} - {searchString} - {searchType}.");
            Search.Remove(DataStore, name, searchString);
        });
    }

    private async Task<IEnumerable<ISearch>> GetAllSearchesAsync()
    {
        return await Task.Run(() =>
        {
            ValidateDataStore();
            return Search.GetAll(DataStore);
        });
    }

    // ISearchRepository implementation
    public ISearch GetSearch(string name, string searchString)
    {
        ValidateDataStore();
        return Search.Get(DataStore, name, searchString) ?? throw new InvalidOperationException($"Search {name} - {searchString} not found.");
    }

    public Task<IEnumerable<ISearch>> GetSavedSearches()
    {
        return GetAllSearchesAsync();
    }

    public Task RemoveSavedSearch(ISearch search)
    {
        return RemoveSearchAsync(search.Name, search.SearchString, search.Type);
    }

    public Task ValidateSearch(ISearch search)
    {
        return ValidateSearch(search.SearchString, search.Type);
    }

    public Task AddSavedSearch(ISearch search)
    {
        return AddSearchAsync(search.Name, search.SearchString, search.Type);
    }
}
