namespace ProcessWatching.ConsoleApp;

using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;

public class Setup
{
    public ProcessWatchingStatus? Status { get; private set; }

    public ILogger? Logger { get; private set; }

    [RequiresUnreferencedCode("Binding of WatchdogOptions")]
    public Watchdog Create(IConfiguration configuration, IVisualizer visualizer)
    {
        var proccessWatchingOptions = configuration.GetSection("ProcessWatching")?.Get<ProcessWatchingOptions>();
        Guard.IsNotNull(proccessWatchingOptions);
        var watchdogOptions = configuration.GetSection("Watchdog")?.Get<WatchdogOptions>();
        Guard.IsNotNull(watchdogOptions);

        ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddFile(configuration.GetSection("Logging"));
        });

        Logger = loggerFactory.CreateLogger("Watchdog");

        Logger.LogInformation("================== Application started. ==================");

        Status = new ProcessWatchingStatus()
        {
            ProcessFile = watchdogOptions.ProcessFile,
        };

        var processWatcher = new ProcessWatcher(proccessWatchingOptions);
        var watchdog = new Watchdog(processWatcher, watchdogOptions);
        watchdog.WatchingStarted += (s, a) =>
        {
            Logger.LogInformation("Started watching.");
            Status.IsWatching = true;
            Status.ProcessInfo = a.ProcessInfo;
            visualizer.Visualize(Status);
        };
        watchdog.WatchingStopped += (s, a) =>
        {
            Logger.LogInformation("Stopped watching.");
            Status.IsWatching = false;
            Status.ProcessInfo = a.ProcessInfo;
            visualizer.Visualize(Status);
        };
        watchdog.ProcessStatusReport += (s, a) =>
        {
            Status.IsWatching = true;
            Status.ProcessInfo = a.ProcessInfo;
            visualizer.Visualize(Status);
        };
        watchdog.ProcessStarted += (s, a) =>
        {
            Logger.LogInformation("Starter process '{ProcessName}', Pid:{ProcessId}.", a.ProcessInfo?.Name, a.ProcessInfo?.Id);
            Status.IsProcessHealthy = true;
            Status.ProcessInfo = a.ProcessInfo;
            visualizer.Visualize(Status);
        };
        watchdog.ProcessError += (s, a) =>
        {
            Logger.LogInformation("Process '{ProcessName}' Pid:{ProcessId} error: '{Message}'.",
                a.ProcessInfo?.Name, a.ProcessInfo?.Id, a.Message);
            Status.IsProcessHealthy = false;
            Status.ProcessInfo = a.ProcessInfo;
            visualizer.Visualize(Status);
        };
        watchdog.ProcessStopped += (s, a) =>
        {
            Logger.LogInformation("Stopped process '{ProcessName}'.", a.ProcessInfo?.Name);
            Status.IsProcessHealthy = false;
            Status.ProcessInfo = a.ProcessInfo;
            visualizer.Visualize(Status);
        };

        return watchdog;
    }
}