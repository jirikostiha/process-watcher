namespace ProcessWatching;

using CommunityToolkit.Diagnostics;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class Watchdog
{
    private DateTimeOffset? _lastProcessStartTime;
    private DateTimeOffset? _lastProcessStopTime;
    private TimeSpan? _lastStartDelay;

    public Watchdog(ProcessWatcher processWatcher, WatchdogOptions options)
    {
        Guard.IsNotNull(processWatcher);
        Guard.IsNotNull(options);
        Guard.IsNotNullOrEmpty(options.ProcessName);
        Guard.IsNotNullOrEmpty(options.ProcessFile);
        Guard.IsTrue(File.Exists(options.ProcessFile));

        Options = options;
        Watcher = processWatcher;

        processWatcher.ProcessStopped += RestartProcessHandler;
    }

    public event EventHandler<ProcessEventArgs>? ProcessStarting;

    public event EventHandler<ProcessEventArgs>? ProcessStarted;

    public event EventHandler<ProcessEventArgs>? ProcessError;

    public event EventHandler<ProcessEventArgs> WatchingStarted
    {
        add { Watcher.WatchingStarted += value; }
        remove { Watcher.WatchingStarted -= value; }
    }

    public event EventHandler<ProcessEventArgs> ProcessStopped
    {
        add { Watcher.ProcessStopped += value; }
        remove { Watcher.ProcessStopped -= value; }
    }

    public event EventHandler<ProcessEventArgs> ProcessStatusReport
    {
        add { Watcher.ProcessStatusReport += value; }
        remove { Watcher.ProcessStatusReport -= value; }
    }

    public event EventHandler<ProcessEventArgs> WatchingStopped
    {
        add { Watcher.WatchingStopped += value; }
        remove { Watcher.WatchingStopped -= value; }
    }

    public WatchdogOptions Options { get; private set; }

    public bool IsWatching => Watcher.IsWatching;

    protected ProcessWatcher Watcher { get; private set; }

    //protected TimeSpan LastProcessUpTime => _lastProcessStopTime - _lastProcessStartTime;

    public async Task StartWatchingAsync(CancellationToken cancellationToken = default)
    {
        if (IsWatching)
            return;

        Reset();
        if (!ProcessWatcher.IsProcessRunning(Options.ProcessName))
            StartProcess(CreateProcess());

        await Watcher.StartWatchingAsync(Options.ProcessName, cancellationToken).ConfigureAwait(false);
    }

    private async void RestartProcessHandler(object? sender, ProcessEventArgs e)
    {
        await Task.Delay(CountDelay());

        if (IsWatching && !ProcessWatcher.IsProcessRunning(Options.ProcessName))
            StartProcess(CreateProcess());
    }

    public void StopWatching()
    {
        Watcher?.StopWatching();
    }

    private Process? StartProcess(Process process)
    {
        try
        {
            ProcessStarting?.Invoke(this, new ProcessEventArgs(process.GetProcessInfo()));

            _lastStartDelay = Options.StartDelay;
            _lastProcessStartTime = TimeProvider.System.GetUtcNow();
            var started = process.Start();

            if (!started)
            {
                ProcessError?.Invoke(this, new ProcessEventArgs(process.GetProcessInfo(), "Process has not started."));

                return process;
            }

            ProcessStarted?.Invoke(this, new ProcessEventArgs(process.GetProcessInfo()));
        }
        catch (Exception ex)
        {
            ProcessError?.Invoke(this, new ProcessEventArgs(process.GetProcessInfo(), ex.Message));
        }

        return process;
    }

    private Process CreateProcess()
    {
        var process = new Process();
        try
        {
            var fileFullPath = Path.GetFullPath(Options.ProcessFile);
            var workingDirectory = Path.GetDirectoryName(fileFullPath);

            var startInfo = new ProcessStartInfo
            {
                FileName = fileFullPath,
                WorkingDirectory = workingDirectory,
                UseShellExecute = true,
                CreateNoWindow = false,
                WindowStyle = ProcessWindowStyle.Normal,
            };

            process.StartInfo = startInfo;
            process.EnableRaisingEvents = true;
            process.Exited += (s, a) =>
            {
                _lastProcessStopTime = TimeProvider.System.GetUtcNow();
                Watcher.OnProcessStopped(new ProcessEventArgs(process.GetProcessInfo(Options.ProcessName)));
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Creating process failed.", ex);
        }

        return process;
    }

    private TimeSpan CountDelay()
    {
        var lastProcessUptime = _lastProcessStopTime - _lastProcessStartTime;

        // process exited after start and in given window - considered as failed too soon
        if (lastProcessUptime.HasValue && lastProcessUptime > TimeSpan.Zero && lastProcessUptime <= Options.StartWindow)
        {
            if (!_lastStartDelay.HasValue)
            {
                _lastStartDelay = Options.StartDelay;
                return _lastStartDelay.Value;
            }

            _lastStartDelay *= Options.DelayCoef;
            return _lastStartDelay.Value;
        }

        return Options.StartDelay;
    }

    private void Reset()
    {
        _lastStartDelay = Options.StartDelay;
        _lastProcessStartTime = null;
        _lastProcessStopTime = null;
    }
}