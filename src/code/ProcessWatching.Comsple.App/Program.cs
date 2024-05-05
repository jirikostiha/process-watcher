namespace ProcessWatching.ConsoleApp;

using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

public static class Program
{
    static async Task Main(string[] args)
    {
        var configFile = "appsettings.json";
        if (args.Length > 1)
            configFile = args[1];

        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile(configFile, optional: false, reloadOnChange: false)
            .Build();

        var proccessWatchingOptions = configuration.GetSection("ProcessWatching")?.Get<ProcessWatchingOptions>();
        Guard.IsNotNull(proccessWatchingOptions);

        var visualizer = new ConsoleVisualizer();
        var status = new ProcessWatchingStatus()
        {
            ProcessFile = proccessWatchingOptions.ProcessFile,
        };

        var watchdog = new Watchdog(proccessWatchingOptions);
        watchdog.WatchingStarted += (s, a) =>
        {
            status.IsWatching = true;
            status.ProcessInfo = a.ProcessInfo;
            visualizer.Visualize(status);
        };
        watchdog.WatchingStopped += (s, a) =>
        {
            status.IsWatching = false;
            status.ProcessInfo = a.ProcessInfo;
            visualizer.Visualize(status);
        };
        watchdog.ProcessStarted += (s, a) =>
        {
            status.IsProcessHealthy = true;
            status.ProcessInfo = a.ProcessInfo;
            visualizer.Visualize(status);
        };
        watchdog.ProcessExited += (s, a) =>
        {
            status.IsProcessHealthy = false;
            status.ProcessInfo = a.ProcessInfo;
            visualizer.Visualize(status);
        };
        watchdog.ProcessError += (s, a) =>
        {
            status.IsProcessHealthy = false;
            status.ProcessInfo = a.ProcessInfo;
            visualizer.Visualize(status);
        };

        visualizer.Visualize(status);

        HandleUserInput(watchdog);

        await Task.Delay(-1);
    }

    private static void HandleUserInput(Watchdog watchdog)
    {
        while (true)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(intercept: true).Key;

                switch (key)
                {
                    case ConsoleKey.Spacebar:
                        {
                            if (!watchdog.IsActive)
                                _ = watchdog.StartWatchingAsync();
                            else
                                watchdog.StopWatching();
                        }
                        break;

                    case ConsoleKey.Escape:
                        Environment.Exit(0);
                        break;
                }
            }

            // Add a delay to reduce CPU usage
            Thread.Sleep(100);
        }
    }
}