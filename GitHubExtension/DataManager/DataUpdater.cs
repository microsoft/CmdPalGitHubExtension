// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Serilog;

namespace GitHubExtension.DataManager;

public class DataUpdater : IDisposable
{
    // This is the default interval the timer will run. It is not the interval that we necessarily do work.
    private static readonly TimeSpan _timerUpdateInterval = TimeSpan.FromSeconds(5);

    private readonly ILogger _logger;
    private readonly PeriodicTimer _timer;
    private readonly Func<Task> _action;
    private CancellationTokenSource _cancelSource;
    private bool _started;

    public bool IsRunning => _started;

    public DataUpdater(TimeSpan interval, Func<Task> action)
    {
        _logger = Log.Logger.ForContext("SourceContext", nameof(DataUpdater));
        _timer = new PeriodicTimer(interval);
        _cancelSource = new CancellationTokenSource();
        _started = false;
        _action = action;
    }

    public DataUpdater(Func<Task> action)
        : this(_timerUpdateInterval, action)
    {
    }

    public async Task Start()
    {
        if (_started)
        {
            // Do nothing if already started.
            return;
        }

        _started = true;
        _cancelSource = new CancellationTokenSource();
        await Task.Run(async () =>
        {
            while (await _timer.WaitForNextTickAsync(_cancelSource.Token))
            {
                await _action();
            }
        });
    }

    public void Stop()
    {
        if (_started)
        {
            _cancelSource.Cancel();
            _started = false;
        }
    }

    public override string ToString() => "DataUpdater";

    private bool _disposed; // To detect redundant calls.

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _logger.Debug("Disposing of all updater resources.");

            if (disposing)
            {
                _timer.Dispose();
            }

            _disposed = true;
        }
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
