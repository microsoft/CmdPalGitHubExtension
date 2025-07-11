// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.Controls;
using GitHubExtension.DataManager.Cache.CacheManagerStates;
using GitHubExtension.DataManager.Data;
using GitHubExtension.DataManager.Enums;
using GitHubExtension.Helpers;
using Serilog;

namespace GitHubExtension.DataManager.Cache;

public sealed class CacheManager : IDisposable, ICacheManager
{
    public static readonly TimeSpan UpdateInterval = ExtensionConstants.UpdateInterval;

    public static readonly TimeSpan RefreshCooldown = ExtensionConstants.RefreshCooldown;

    // Lock to be used everytime we want to check or update the state of
    // the CacheManager.
    private readonly SemaphoreSlim _stateSemaphore = new(1, 1);

    private readonly ILogger _logger;

    public CacheManagerState State { get; set; }

    public CacheManagerState IdleState { get; private set; }

    public CacheManagerState RefreshingState { get; private set; }

    public CacheManagerState PeriodicUpdatingState { get; private set; }

    public CacheManagerState PendingRefreshState { get; private set; }

    public CacheManagerState PendingClearCacheState { get; private set; }

    private readonly IGitHubCacheDataManager _dataManager;
    private readonly ISearchRepository _searchRepository;
    private readonly AuthenticationMediator _authenticationMediator;

    private CancellationTokenSource _cancelSource;

    // Variables to control the state of the CacheManager
    // If there is a current update in progress
    public ISearch? PendingSearch { get; internal set; }

    // The type of update that is currently in progress
    public UpdateType CurrentUpdateType { get; set; }

    public bool NeverUpdated => LastUpdated == DateTime.MinValue;

    // Time of the last update. This is updated by the
    // Cache Manager whe it receives an update complete event.
    public DateTime LastUpdated { get => GetLastUpdated(); private set => SetLastUpdated(value); }

    public event CacheManagerUpdateEventHandler? OnUpdate;

    private DataUpdater DataUpdater { get; set; }

    public DateTime LastUpdateTime { get; set; } = DateTime.MinValue;

    public CacheManager(IGitHubCacheDataManager dataManager, ISearchRepository searchRepository, AuthenticationMediator authenticationMediator)
    {
        _dataManager = dataManager;
        _searchRepository = searchRepository;
        _authenticationMediator = authenticationMediator;
        _authenticationMediator.SignOutAction += ClearCache;
        DataUpdater = new DataUpdater(PeriodicUpdate);
        dataManager.OnUpdate += HandleDataManagerUpdate;
        _cancelSource = new CancellationTokenSource();
        _logger = Log.Logger.ForContext("SourceContext", nameof(CacheManager));

        // Setting states
        IdleState = new IdleState(this);
        RefreshingState = new RefreshingState(this);
        PeriodicUpdatingState = new PeriodicUpdatingState(this);
        PendingRefreshState = new PendingRefreshState(this);
        PendingClearCacheState = new PendingClearCacheState(this);
        State = IdleState;
    }

    public void Start()
    {
        _ = DataUpdater.Start();
    }

    public void Stop()
    {
        DataUpdater.Stop();
    }

    private async Task SemaphoreWrapper(Func<Task> stateProcedure)
    {
        await _stateSemaphore.WaitAsync();
        try
        {
            await stateProcedure();
        }
        finally
        {
            _stateSemaphore.Release();
        }
    }

    private async void ClearCache(object? sender, SignInStatusChangedEventArgs e)
    {
        await SemaphoreWrapper(() =>
        {
            _logger.Information("Purging all data.");
            State.ClearCache();
            return Task.CompletedTask;
        });
    }

    public void PurgeAllData()
    {
        _dataManager.PurgeAllData();
    }

    public void CancelUpdateInProgress()
    {
        if (!_cancelSource.IsCancellationRequested)
        {
            _logger.Information("Cancelling update.");
            _cancelSource.Cancel();
        }
    }

    public async Task RequestRefresh(ISearch search)
    {
        if (_dataManager.IsSearchNewOrStale(search, RefreshCooldown))
        {
            _logger.Information("Search is stale or new. Refreshing.");
            await Refresh(search);
        }
    }

    // This method is called by the pages to request
    // an instant update of its data.
    public async Task Refresh(ISearch search)
    {
        await SemaphoreWrapper(async () => await State.Refresh(search));
    }

    public async Task PeriodicUpdate()
    {
        await SemaphoreWrapper(async () => await State.PeriodicUpdate());
    }

    public async Task Update(UpdateType updateType, ISearch? search = null)
    {
        var options = new RequestOptions();

        _logger.Information($"Starting update of type {updateType}.");

        lock (_stateSemaphore)
        {
            _cancelSource = new CancellationTokenSource();
            options.CancellationToken = _cancelSource.Token;
        }

        // Do the update for saved queries here
        _logger.Debug($"Starting update of type {updateType}.");

        switch (updateType)
        {
            case UpdateType.All:
                var searches = (await _searchRepository.GetSavedSearches()).ToList();
                _ = _dataManager.RequestAllUpdateAsync(searches, options);
                break;
            case UpdateType.Search:
                _ = _dataManager.RequestSearchUpdateAsync(search!, options);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(updateType), updateType, null);
        }
    }

    public void SendUpdateEvent(object? source, CacheManagerUpdateKind kind, ISearch? search = null, Exception? ex = null)
    {
        _logger.Debug($"Sending update event. Kind: {kind}.");
        OnUpdate?.Invoke(source, new CacheManagerUpdateEventArgs(kind, search, ex));
    }

    private async void HandleDataManagerUpdate(object? source, DataManagerUpdateEventArgs e)
    {
        _logger.Information($"DataManager update: {e.Kind}, {e.UpdateType}");
        await SemaphoreWrapper(() =>
        {
            State.HandleDataManagerUpdate(source, e);
            return Task.CompletedTask;
        });

        switch (e.Kind)
        {
            case DataManagerUpdateKind.Success:
                SendUpdateEvent(this, CacheManagerUpdateKind.Updated, e.Search);
                break;
            case DataManagerUpdateKind.Cancel:
                SendUpdateEvent(this, CacheManagerUpdateKind.Cancel, e.Search);
                break;
            case DataManagerUpdateKind.Error:
                SendUpdateEvent(this, CacheManagerUpdateKind.Error, e.Search, e.Exception);
                break;
        }
    }

    private DateTime GetLastUpdated()
    {
        var lastCacheUpdate = _dataManager.LastUpdated;
        if (lastCacheUpdate != null)
        {
            return lastCacheUpdate;
        }

        return DateTime.MinValue;
    }

    private void SetLastUpdated(DateTime time)
    {
        _dataManager.LastUpdated = time;
    }

    // Disposing area
    private bool _disposed;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _logger.Debug("Disposing of all cacheManager resources.");

            if (disposing)
            {
                try
                {
                    _logger.Debug("Disposing of all CacheManager resources.");
                    _dataManager.OnUpdate -= HandleDataManagerUpdate;
                    _authenticationMediator.SignOutAction -= ClearCache;
                    DataUpdater.Dispose();
                    _cancelSource.Dispose();
                    _stateSemaphore.Dispose();
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Failed disposing");
                }
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
