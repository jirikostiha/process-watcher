namespace ProcessWatching.ConsoleApp;

using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Configuration;

public class Setup
{
    public ProcessWatchingStatus Status { get; set; }

    public Watchdog Create(IConfiguration configuration, ConsoleVisualizer visualizer)
    {
        var proccessWatchingOptions = configuration.GetSection("ProcessWatching")?.Get<ProcessWatchingOptions>();
        Guard.IsNotNull(proccessWatchingOptions);
        var watchdogOptions = configuration.GetSection("Watchdog")?.Get<WatchdogOptions>();
        Guard.IsNotNull(watchdogOptions);

        Status = new ProcessWatchingStatus()
        {
            ProcessFile = watchdogOptions.ProcessFile,
        };

        var processWatcher = new ProcessWatcher(proccessWatchingOptions);
        var watchdog = new Watchdog(processWatcher, watchdogOptions);
        watchdog.WatchingStarted += (s, a) =>
        {
            Status.IsWatching = true;
            Status.ProcessInfo = a.ProcessInfo;
            visualizer.Visualize(Status);
        };
        watchdog.WatchingStopped += (s, a) =>
        {
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
            Status.IsProcessHealthy = true;
            Status.ProcessInfo = a.ProcessInfo;
            visualizer.Visualize(Status);
        };
        watchdog.ProcessError += (s, a) =>
        {
            Status.IsProcessHealthy = false;
            Status.ProcessInfo = a.ProcessInfo;
            visualizer.Visualize(Status);
        };
        watchdog.ProcessStopped += (s, a) =>
        {
            Status.ProcessInfo = a.ProcessInfo;
            visualizer.Visualize(Status);
        };

        return watchdog;
    }
}