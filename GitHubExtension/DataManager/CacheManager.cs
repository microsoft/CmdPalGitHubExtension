// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using GitHubExtension.DataModel.Enums;
using Serilog;

namespace GitHubExtension.DataManager;

public class CacheManager : IDisposable
{
    private static readonly TimeSpan _updateInterval = TimeSpan.FromMinutes(10);

    private static readonly TimeSpan _updateFrequency = TimeSpan.FromMinutes(5);

    private static readonly object _instanceLock = new();

    // Lock to be used everytime we want to check or update the state of
    // the variables: UpdateInProgress, _currentRefreshKind, _pendingRefresh
    // and LastUpdated.
    private static readonly object _stateLock = new();

    private static CacheManager? _singletonInstance;

    private readonly ILogger _logger;

    private CancellationTokenSource _cancelSource;

    private IGitHubDataManager DataManager { get; set; }

    private GitHubRepositoryHelper RepositoryHelper { get; set; }

    public bool UpdateInProgress { get; private set; }

    private RefreshKind _currentRefreshKind;

    public bool NeverUpdated => LastUpdated == DateTime.MinValue;

    public DateTime LastUpdated { get => GetLastUpdated(); private set => SetLastUpdated(value); }

    // If a refresh call is pending and has not yet completed
    private bool _pendingRefresh;

    public event CacheManagerUpdateEventHandler? OnUpdate;

    private DataUpdater DataUpdater { get; set; }

    private DateTime LastUpdateTime { get; set; } = DateTime.MinValue;

    public static CacheManager GetInstance()
    {
        try
        {
            lock (_instanceLock)
            {
                _singletonInstance ??= new CacheManager();
            }

            return _singletonInstance;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed creating CacheManager.");
            throw;
        }
    }

    private CacheManager()
    {
        DataManager = GitHubDataManager.CreateInstance() ?? throw new DataStoreInaccessibleException();
        DataUpdater = new DataUpdater(PeriodicUpdate);
        RepositoryHelper = GitHubRepositoryHelper.Instance;
        GitHubDataManager.OnUpdate += HandleDataManagerUpdate;
        _cancelSource = new CancellationTokenSource();
        _logger = Log.Logger.ForContext("SourceContext", nameof(CacheManager));
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
            if (UpdateInProgress && !_cancelSource.IsCancellationRequested)
            {
                _logger.Information("Cancelling update.");
                _cancelSource.Cancel();
            }
        }
    }

    public async Task Refresh(RefreshKind kind)
    {
        CancelUpdateInProgress();

        lock (_stateLock)
        {
            if (_pendingRefresh && _currentRefreshKind == kind)
            {
                _logger.Debug("Refresh of this type already pending. Ignoring refresh request.");
                return;
            }

            _pendingRefresh = true;
            _currentRefreshKind = kind;
        }

        await Update(TimeSpan.MinValue, kind);
    }

    private async Task PeriodicUpdate()
    {
        // Only update per the update interval.
        if (DateTime.UtcNow - LastUpdateTime < _updateInterval)
        {
            return;
        }

        try
        {
            await Update(_updateFrequency, RefreshKind.All);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Update failed unexpectedly");
        }

        LastUpdateTime = DateTime.UtcNow;
        return;
    }

    private async Task Update(TimeSpan? olderThan, RefreshKind kind, PersistentData.Search? search = null)
    {
        var options = new RequestOptions();

        _logger.Information($"Starting update of kind {kind}.");

        lock (_stateLock)
        {
            if (UpdateInProgress)
            {
                // If we enter here after a refresh request, it means that
                // the update is in progress and still has not been canceled.
                // We ignore it for now, but as the _pendingRefresh is true,
                // once we get the Cancel update from the DataManager, we will
                // start a new update for this refresh request.
                _logger.Information("Update in progress, ignoring request.");
                return;
            }

            UpdateInProgress = true;
            _cancelSource = new CancellationTokenSource();
            options.CancellationToken = _cancelSource.Token;

            // Limiting to 100 for now for performance reasons.
            options.ApiOptions.PageSize = 100;
            options.ApiOptions.PageCount = 1;
        }

        // Do the update for saved queries here
        _logger.Debug($"Starting update of kind {kind}.");
        _currentRefreshKind = kind;

        var repoCollection = RepositoryHelper.GetUserRepositoryCollection();
        if (kind == RefreshKind.All)
        {
            await DataManager.UpdateAllDataForRepositoriesAsync(repoCollection, options);

            var searches = new List<PersistentData.Search>();
            await DataManager.UpdateDataForSearchesAsync(searches, options);
        }

        if (kind == RefreshKind.Issues)
        {
            await DataManager.UpdateIssuesForRepositoriesAsync(repoCollection, options);
        }

        if (kind == RefreshKind.PullRequests)
        {
            await DataManager.UpdatePullRequestsForRepositoriesAsync(repoCollection, options);
        }

        if (kind == RefreshKind.Search)
        {
            await DataManager.UpdateDataForSearchAsync(search!.Name, search!.SearchString, (SearchType)search!.TypeId, options);
        }
    }

    private void SendUpdateEvent(object? source, CacheManagerUpdateKind kind, Exception? ex = null)
    {
        if (OnUpdate != null)
        {
            _logger.Debug($"Sending update event. Kind: {kind}.");
            OnUpdate.Invoke(source, new CacheManagerUpdateEventArgs(kind, ex));
        }
    }

    private void HandleDataManagerUpdate(object? source, DataManagerUpdateEventArgs e)
    {
        _logger.Information($"DataManager update: {e.Kind}");

        if (e.Kind == DataManagerUpdateKind.PullRequests)
        {
            if (_currentRefreshKind == RefreshKind.PullRequests)
            {
                lock (_stateLock)
                {
                    UpdateInProgress = false;
                    _pendingRefresh = false;
                    LastUpdated = DateTime.UtcNow;
                }
            }

            SendUpdateEvent(this, CacheManagerUpdateKind.Updated);
        }

        if (e.Kind == DataManagerUpdateKind.Issues)
        {
            if (_currentRefreshKind == RefreshKind.Issues)
            {
                lock (_stateLock)
                {
                    UpdateInProgress = false;
                    _pendingRefresh = false;
                    LastUpdated = DateTime.UtcNow;
                }
            }

            SendUpdateEvent(this, CacheManagerUpdateKind.Updated);
        }

        if (e.Kind == DataManagerUpdateKind.All)
        {
            if (_currentRefreshKind == RefreshKind.All)
            {
                lock (_stateLock)
                {
                    UpdateInProgress = false;
                }
            }

            if (_pendingRefresh)
            {
                _ = Update(TimeSpan.MinValue, _currentRefreshKind);
            }

            SendUpdateEvent(this, CacheManagerUpdateKind.Updated);
        }

        if (e.Kind == DataManagerUpdateKind.Cancel)
        {
            lock (_stateLock)
            {
                UpdateInProgress = false;
            }

            _logger.Debug("Received cancel event from DataManager.");
            SendUpdateEvent(this, CacheManagerUpdateKind.Cancel);

            if (_pendingRefresh)
            {
                // If there is a pending refresh, it is likely because a
                // refresh request caused this cancellation. And as a race
                // between the previous update happening and the new update trying
                // to start and failing because of the update in progress, we will
                // need to start the new update for that refresh request.
                _ = Update(TimeSpan.MinValue, _currentRefreshKind);
            }
        }

        if (e.Kind == DataManagerUpdateKind.Error)
        {
            lock (_stateLock)
            {
                UpdateInProgress = false;
                _pendingRefresh = false;
            }

            SendUpdateEvent(this, CacheManagerUpdateKind.Error);
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
                    DataManager.Dispose();
                    _cancelSource.Dispose();
                    GitHubDataManager.OnUpdate -= HandleDataManagerUpdate;
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
