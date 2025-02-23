// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataManager.CacheManagerStates;
using GitHubExtension.Helpers;
using Serilog;

namespace GitHubExtension.DataManager;

public sealed class CacheManager : ICacheManager, IDisposable
{
    public static readonly TimeSpan UpdateInterval = TimeSpan.FromMinutes(10);

    public static readonly TimeSpan UpdateFrequency = TimeSpan.FromMinutes(5);

    private static readonly object _instanceLock = new();

    // Lock to be used everytime we want to check or update the state of
    // the CacheManager.
    private static readonly object _stateLock = new();

    private readonly ILogger _logger;

    public CacheManagerState State { get; internal set; }

    public object GetStateLock()
    {
        return _stateLock;
    }

    public CacheManagerState IdleState { get; private set; }

    public CacheManagerState RefreshingState { get; private set; }

    public CacheManagerState PeriodicUpdatingState { get; private set; }

    public CacheManagerState PendingRefreshState { get; private set; }

    private CancellationTokenSource _cancelSource;

    public IGitHubDataManager DataManager { get; private set; }

    private IRepositoryHelper RepositoryHelper { get; set; }

    private ISearchHelper SearchHelper { get; set; }

    // Variables to control the state of the CacheManager
    // If there is a current update in progress
    public PersistentData.Search? PendingSearch { get; set; }

    // The type of update that is currently in progress
    public UpdateType CurrentUpdateType { get; set; }

    public bool NeverUpdated => LastUpdated == DateTime.MinValue;

    // Time of the last update. This is updated by the
    // Cache Manager whe it receives an update complete event.
    public DateTime LastUpdated { get => GetLastUpdated(); private set => SetLastUpdated(value); }

    private CacheManagerUpdateEventHandler? _onUpdate;

    public event CacheManagerUpdateEventHandler? OnUpdate
    {
        add
        {
            lock (_stateLock)
            {
                // Ensuring only one page is listeing to the event.
                _onUpdate = value;
            }
        }

        remove
        {
            lock (_stateLock)
            {
                _onUpdate -= value;
            }
        }
    }

    private DataUpdater DataUpdater { get; set; }

    public DateTime LastUpdateTime { get; set; } = DateTime.MinValue;

    public CacheManager(IGitHubDataManager dataManager, IRepositoryHelper repositoryHelper, ISearchHelper searchHelper)
    {
        // Injected dependencies
        DataManager = dataManager;
        RepositoryHelper = repositoryHelper;
        SearchHelper = searchHelper;

        DataUpdater = new DataUpdater(PeriodicUpdate);
        dataManager.OnUpdate += HandleDataManagerUpdate;
        _cancelSource = new CancellationTokenSource();
        _logger = Log.Logger.ForContext("SourceContext", nameof(CacheManager));

        // Setting states
        IdleState = new IdleState(this);
        RefreshingState = new RefreshingState(this);
        PeriodicUpdatingState = new PeriodicUpdatingState(this);
        PendingRefreshState = new PendingRefreshState(this);
        State = IdleState;

        Start();
    }

    public void Start()
    {
        _ = DataUpdater.Start();
    }

    public void Stop()
    {
        DataUpdater.Stop();
    }

    public void CancelUpdateInProgress()
    {
        lock (_stateLock)
        {
            if (!_cancelSource.IsCancellationRequested)
            {
                _logger.Information("Cancelling update.");
                _cancelSource.Cancel();
            }
        }
    }

    // This method is called by the pages to request
    // an instant update of its data.
    public async Task Refresh(UpdateType updateType, PersistentData.Search? search = null)
    {
        await State.Refresh(updateType, search);
    }

    public async Task PeriodicUpdate()
    {
        await State.PeriodicUpdate();
    }

    public async Task Update(TimeSpan? olderThan, UpdateType updateType, PersistentData.Search? search = null)
    {
        var options = new RequestOptions();

        _logger.Information($"Starting update of type {updateType}.");

        lock (_stateLock)
        {
            _cancelSource = new CancellationTokenSource();
            options.CancellationToken = _cancelSource.Token;

            // Limiting to 100 for now for performance reasons.
            options.ApiOptions.PageSize = 100;
            options.ApiOptions.PageCount = 1;
        }

        // Do the update for saved queries here
        _logger.Debug($"Starting update of type {updateType}.");

        var repoCollection = RepositoryHelper.GetUserRepositoryCollection();
        switch (updateType)
        {
            case UpdateType.All:
                var searches = (await SearchHelper.GetSavedSearches()).ToList();
                await DataManager.RequestAllUpdateAsync(repoCollection, searches, options);
                break;
            case UpdateType.Issues:
                await DataManager.RequestIssuesUpdateAsync(repoCollection, options);
                break;
            case UpdateType.PullRequests:
                await DataManager.RequestPullRequestsUpdateAsync(repoCollection, options);
                break;
            case UpdateType.Search:
                await DataManager.RequestSearchUpdateAsync(search!.Name, search!.SearchString, search!.Type, options);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(updateType), updateType, null);
        }
    }

    public void SendUpdateEvent(object? source, CacheManagerUpdateKind kind, Exception? ex = null)
    {
        if (_onUpdate != null)
        {
            _logger.Debug($"Sending update event. Kind: {kind}.");
            _onUpdate.Invoke(source, new CacheManagerUpdateEventArgs(kind, ex));
        }
    }

    private void HandleDataManagerUpdate(object? source, DataManagerUpdateEventArgs e)
    {
        _logger.Information($"DataManager update: {e.Kind}, {e.UpdateType}");
        State.HandleDataManagerUpdate(source, e);

        switch (e.Kind)
        {
            case DataManagerUpdateKind.Success:
                SendUpdateEvent(this, CacheManagerUpdateKind.Updated);
                break;
            case DataManagerUpdateKind.Cancel:
                SendUpdateEvent(this, CacheManagerUpdateKind.Cancel);
                break;
            case DataManagerUpdateKind.Error:
                SendUpdateEvent(this, CacheManagerUpdateKind.Error);
                break;
        }
    }

    private DateTime GetLastUpdated()
    {
        var lastCacheUpdate = DataManager.LastUpdated;
        if (lastCacheUpdate != null)
        {
            return lastCacheUpdate;
        }

        return DateTime.MinValue;
    }

    private void SetLastUpdated(DateTime time)
    {
        DataManager.LastUpdated = time;
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
                    DataUpdater.Dispose();
                    DataManager.OnUpdate -= HandleDataManagerUpdate;
                    DataManager.Dispose();
                    _cancelSource.Dispose();
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
