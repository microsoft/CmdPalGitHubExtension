﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.DataManager.Data;
using GitHubExtension.DataModel;
using GitHubExtension.DataModel.Enums;
using Serilog;
using Windows.Storage;

namespace GitHubExtension.PersistentData;

public class PersistentDataManager : IDisposable, ISearchRepository
{
    private static readonly Lazy<ILogger> _logger = new(() => Serilog.Log.ForContext("SourceContext", nameof(PersistentDataManager)));

    private static readonly ILogger _log = _logger.Value;

    private const string DataStoreFileName = "PersistentGitHubData.db";

    private readonly IGitHubValidator _gitHubValidator;

    private DataStore DataStore { get; set; }

    public DataStoreOptions DataStoreOpions { get; set; }

    private void ValidateDataStore()
    {
        if (DataStore == null || !DataStore.IsConnected)
        {
            throw new DataStoreInaccessibleException("DataStore is not available.");
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

    public PersistentDataManager(IGitHubValidator gitHubValidator, DataStoreOptions? dataStoreOptions = null)
    {
        _gitHubValidator = gitHubValidator;

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

    public async Task<IEnumerable<Repository>> GetAllRepositoriesAsync()
    {
        return await Task.Run(() =>
        {
            ValidateDataStore();
            return Repository.GetAll(DataStore);
        });
    }

    private Task AddSearchAsync(ISearch search)
    {
        return Task.Run(() =>
        {
            ValidateDataStore();

            var name = search.Name;
            var searchString = search.SearchString;
            var searchType = search.Type;

            _log.Information($"Adding search: {name} - {searchString} - {searchType}.");
            if (Search.Get(DataStore, name, searchString) != null)
            {
                throw new InvalidOperationException($"Search {name} - {searchString} - {searchType} already exists.");
            }

            Search.Add(DataStore, name, searchString);
        });
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

    public async Task ValidateSearch(ISearch search)
    {
        await _gitHubValidator.ValidateSearch(search);
    }

    public async Task AddSavedSearch(ISearch search)
    {
        await ValidateSearch(search);
        await AddSearchAsync(search);
    }
}
