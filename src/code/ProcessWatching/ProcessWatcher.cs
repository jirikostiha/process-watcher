namespace ProcessWatching;

using CommunityToolkit.Diagnostics;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Process watcher watching process(es) by name.
/// </summary>
public class ProcessWatcher : IDisposable
{
    private CancellationTokenSource? _cancellationTokenSource;

    public ProcessWatcher(ProcessWatchingOptions options)
    {
        Guard.IsNotNull(options);
        Guard.IsGreaterThanOrEqualTo(options.CheckingPeriod, TimeSpan.Zero);

        Options = options;
    }

    public event EventHandler<ProcessEventArgs>? WatchingStarted;

    public event EventHandler<ProcessEventArgs>? ProcessStopped;

    public event EventHandler<ProcessEventArgs>? ProcessStatusReport;

    public event EventHandler<ProcessEventArgs>? WatchingStopped;

    public ProcessWatchingOptions Options { get; private set; }

    public string? WatchingProcessName { get; private set; }

    public bool IsWatching => WatchingProcessName != null;

    public async Task StartWatchingAsync(string processName, CancellationToken cancellationToken = default)
    {
        Guard.IsNotNullOrEmpty(processName);

        if (IsWatching)
            return;

        WatchingProcessName = processName;

        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        Process? process = null;

        WatchingStarted?.Invoke(this, new ProcessEventArgs(process.GetProcessInfo(WatchingProcessName)));

        try
        {
            while (true)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                    break;

                process = GetProcessByName(WatchingProcessName);

                if (IsProcessRunning(process))
                {
                    ProcessStatusReport?.Invoke(this, new ProcessEventArgs(process.GetProcessInfo(WatchingProcessName)));
                }
                else
                {
                    ProcessStopped?.Invoke(this, new ProcessEventArgs(process.GetProcessInfo(WatchingProcessName)));
                    OnProcessStopped(new ProcessEventArgs(process.GetProcessInfo(WatchingProcessName)));
                }

                // Delay to reduce CPU usage
                await Task.Delay(Options.CheckingPeriod, _cancellationTokenSource.Token);
            }
        }
        catch (Exception)
        {
            // ?
        }

        var watchingProcessName = WatchingProcessName;
        WatchingProcessName = null;

        WatchingStopped?.Invoke(this, new ProcessEventArgs(process.GetProcessInfo(watchingProcessName)));
    }

    public void StopWatching()
    {
        _cancellationTokenSource?.Cancel();
    }

    internal virtual void OnProcessStopped(ProcessEventArgs e)
    {
        ProcessStopped?.Invoke(this, e);
    }

    protected static Process? GetProcessByName(string processName)
    {
        Process[] processes = Process.GetProcessesByName(processName);
        return processes.Length > 0 ? processes[0] : null;
    }

    public static bool IsProcessRunning(string processName) => IsProcessRunning(GetProcessByName(processName));

    public static bool IsProcessRunning(Process? process) => process is not null && !process.HasExited;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        _cancellationTokenSource?.Dispose();
    }
}