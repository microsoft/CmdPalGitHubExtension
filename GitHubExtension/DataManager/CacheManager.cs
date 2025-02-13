// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Serilog;

namespace GitHubExtension.DataManager;

public class CacheManager : IDisposable
{
    private static readonly TimeSpan _updateInterval = TimeSpan.FromMinutes(10);

    private static readonly TimeSpan _updateFrequency = TimeSpan.FromMinutes(5);

    private static readonly object _instanceLock = new();

    private static readonly object _stateLock = new();

    private static CacheManager? _singletonInstance;

    private CancellationTokenSource _cancelSource;

    private IGitHubDataManager DataManager { get; set; }

    private GitHubRepositoryHelper RepositoryHelper { get; set; }

    public bool UpdateInProgress { get; private set; }

    public bool NeverUpdated => LastUpdated == DateTime.MinValue;

    public DateTime LastUpdated { get => GetLastUpdated(); private set => SetLastUpdated(value); }

    // If the next update should clear sync data and force an update
    private bool _clearNextDataUpdate;

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
                Log.Information("Cancelling update.");
                _cancelSource.Cancel();
            }
        }
    }

    public async Task Refresh()
    {
        CancelUpdateInProgress();

        lock (_stateLock)
        {
            if (_pendingRefresh)
            {
                Log.Debug("Refresh already pending. Ignoring refresh request.");
                return;
            }

            _pendingRefresh = true;
            _clearNextDataUpdate = true;
        }

        await Update(TimeSpan.MinValue);
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
            await Update(_updateFrequency);
        }
        catch (Exception e)
        {
            Log.Error(e, "Update failed unexpectedly");
        }

        LastUpdateTime = DateTime.UtcNow;
        return;
    }

    private async Task Update(TimeSpan? olderThan)
    {
        var options = new RequestOptions();

        lock (_stateLock)
        {
            if (UpdateInProgress)
            {
                Log.Information("Update in progress, ignoring request.");
                return;
            }

            UpdateInProgress = true;
            _cancelSource = new CancellationTokenSource();
            options.CancellationToken = _cancelSource.Token;
            options.Refresh = _clearNextDataUpdate;
        }

        // do the update for saved queries here
        Log.Debug("Starting update");
        var repoCollection = RepositoryHelper.GetUserRepositoryCollection();
        await DataManager.UpdateIssuesForRepositoriesAsync(repoCollection, options);
        await DataManager.UpdatePullRequestsForRepositoriesAsync(repoCollection, options);
    }

    private void SendUpdateEvent(object? source, CacheManagerUpdateKind kind, Exception? ex = null)
    {
        if (OnUpdate != null)
        {
            Log.Debug($"Sending update event. Kind: {kind}.");
            OnUpdate.Invoke(source, new CacheManagerUpdateEventArgs(kind, ex));
        }
    }

    private void HandleDataManagerUpdate(object? source, DataManagerUpdateEventArgs e)
    {
        if (e.Kind == DataManagerUpdateKind.Repository)
        {
            SendUpdateEvent(this, CacheManagerUpdateKind.Updated);
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

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
