namespace ProcessWatching.ConsoleApp;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

public class Program
{
    static Setup Setup { get; set; }

    static async Task Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            var ex = (Exception)e.ExceptionObject;
            if (Setup?.Logger is not null)
                Setup.Logger.LogCritical(ex, "Unhandled Exception occurred.");
        };

        var configFile = "appsettings.json";
        if (args.Length > 1)
            configFile = args[1];

        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile(configFile, optional: false, reloadOnChange: false)
            .Build();

        var visualizer = new ConsoleVisualizer();
        Setup = new Setup();
        var watchdog = Setup.Create(configuration, visualizer);
        Setup.Status.ProcessInfo.Name = watchdog.Options.ProcessName;

        visualizer.Visualize(Setup.Status);

        HandleUserInput(watchdog);

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
                            if (!watchdog.IsActive)
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