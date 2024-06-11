namespace ProcessWatching.ConsoleApp;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

public class Program
{
    private static Setup Setup { get; set; }

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