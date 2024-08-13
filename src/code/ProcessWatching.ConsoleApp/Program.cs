namespace ProcessWatching.ConsoleApp;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

public static class Program
{
    private static Setup? Setup { get; set; }

    [RequiresUnreferencedCode("Calls ProcessWatching.ConsoleApp.Setup.Create(IConfiguration, ConsoleVisualizer)")]
    private static async Task Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            var ex = (Exception)e.ExceptionObject;
            if (Setup?.Logger is not null)
            {
                Setup.Logger.LogCritical(ex, "Unhandled Exception. Runtime is terminating {IsTerminating}. sender:'{Sender}'",
                    e.IsTerminating, sender?.ToString() ?? string.Empty);
            }
        };

        AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
        {
            if (Setup?.Logger is not null)
            {
                Setup.Logger.LogInformation("Exiting process. sender:'{Sender}'", sender?.ToString() ?? string.Empty);
            }
        };

        var customConfigFile = "appsettings.json";
        if (args.Length > 1)
            customConfigFile = args[1];

        var configBuilder = new ConfigurationBuilder();
        var currentDir = Directory.GetCurrentDirectory();
        var allParents = currentDir.GetParents().Reverse().ToArray();
        var appName = Process.GetCurrentProcess().ProcessName.ToLower(CultureInfo.InvariantCulture);

        configBuilder.Sources.Clear();

        foreach (var dir in allParents)
        {
            configBuilder.SetBasePath(dir);
            configBuilder.AddJsonFile(customConfigFile, optional: true, reloadOnChange: false)
                .AddJsonFile(appName + ".json", optional: true, reloadOnChange: false);
        }

        var configuration = configBuilder.Build();

        //var visualizer = new SystemConsoleVisualizer();
        var visualizer = new SpectreVisualizer();
        Setup = new Setup();
        var watchdog = Setup.Create(configuration, visualizer);
        if (Setup.Status?.ProcessInfo is not null)
        {
            Setup.Status.ProcessInfo.Name = watchdog.Options.ProcessName;

            visualizer.Visualize(Setup.Status);

            HandleUserInput(watchdog);
        }

        await Task.Delay(-1);
    }

    [SuppressMessage("Blocker Bug", "S2190:Loops and recursions should not be infinite", Justification = "<Pending>")]
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
                            if (!watchdog.IsWatching)
                            {
                                try
                                {
                                    _ = watchdog.StartWatchingAsync();
                                }
                                catch (Exception ex)
                                {
                                    throw new InvalidOperationException("Watching failed.", ex);
                                }
                            }
                            else
                            {
                                watchdog.StopWatching();
                            }
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