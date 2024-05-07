namespace ProcessWatching;

using CommunityToolkit.Diagnostics;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class Watchdog : IDisposable
{
    private CancellationTokenSource? _cancellationTokenSource;

    public Watchdog(ProcessWatchingOptions options)
    {
        Guard.IsNotNull(options);
        Guard.IsNotNullOrEmpty(options.ProcessFile);
        Guard.IsGreaterThanOrEqualTo(options.CheckingPeriod, TimeSpan.Zero);
        Guard.IsGreaterThanOrEqualTo(options.StartDelay, TimeSpan.Zero);
        Guard.IsTrue(File.Exists(options.ProcessFile));

        Options = options;
    }

    public event EventHandler<ProcessEventArgs>? WatchingStarted;
    public event EventHandler<ProcessEventArgs>? ProcessStarting;
    public event EventHandler<ProcessEventArgs>? ProcessStarted;
    public event EventHandler<ProcessEventArgs>? ProcessExited;
    public event EventHandler<ProcessEventArgs>? ProcessError;
    public event EventHandler<ProcessEventArgs>? WatchingStopped;

    public ProcessWatchingOptions Options { get; private set; }

    public bool IsActive { get; private set; }

    public async Task StartWatchingAsync(CancellationToken cancellationToken = default)
    {
        if (IsActive)
            return;

        IsActive = true;

        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        await StartWatchingCoreAsync(_cancellationTokenSource.Token);
    }

    private async Task StartWatchingCoreAsync(CancellationToken cancellationToken = default)
    {
        var processName = GetWatchingProcessName();
        var process = GetProcessByName(processName);

        WatchingStarted?.Invoke(this, new ProcessEventArgs(new()
        {
            Name = process?.ProcessName ?? processName,
            Id = process?.Id,
        }));

        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!IsProcessRunning(processName))
                {
                    await Task.Delay(Options.StartDelay, cancellationToken);

                    process = StartProcess(CreateProcess());
                }

                // Delay to reduce CPU usage
                await Task.Delay(Options.CheckingPeriod, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Cancellation requested
        }

        WatchingStopped?.Invoke(this, new ProcessEventArgs(new()
        {
            Name = process?.ProcessName ?? processName,
            Id = process?.Id
        }));

        IsActive = false;
    }

    public void StopWatching()
    {
        _cancellationTokenSource?.Cancel();
    }

    private string GetWatchingProcessName() => Options.ProcessName is not null
        ? Options.ProcessName
        : Path.GetFileNameWithoutExtension(Options.ProcessFile);

    private static Process? GetProcessByName(string processName)
    {
        Process[] processes = Process.GetProcessesByName(processName);
        return processes.Length > 0 ? processes[0] : null;
    }

    private static bool IsProcessRunning(string processName) => IsProcessRunning(GetProcessByName(processName));

    private static bool IsProcessRunning(Process? process) => process is not null && !process.HasExited;

    private Process? StartProcess(Process process)
    {
        try
        {
            ProcessStarting?.Invoke(this, new ProcessEventArgs(CollectProcessInfo(process)));

            var started = process.Start();

            if (!started)
            {
                ProcessError?.Invoke(this, new ProcessEventArgs(CollectProcessInfo(process), "Process has not started."));

                return process;
            }

            ProcessStarted?.Invoke(this, new ProcessEventArgs(CollectProcessInfo(process)));
        }
        catch (Exception ex)
        {
            ProcessError?.Invoke(this, new ProcessEventArgs(CollectProcessInfo(process), ex.Message));
        }

        return process;
    }

    private static ProcessInfo CollectProcessInfo(Process process)
    {
        return new ProcessInfo()
        {
            Name = process.ProcessName,
            Id = process.Id
        };
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
            process.Exited += (s, a) => ProcessExited?.Invoke(this, new ProcessEventArgs(CollectProcessInfo(process)));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Creating process failed.", ex);
        }

        return process;
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
        GC.SuppressFinalize(this);
    }
}