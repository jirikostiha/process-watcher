using System;

namespace ProcessWatching.ConsoleApp;

public class ConsoleVisualizer
{
    public ConsoleColor WatchingEnabledColor { get; set; } = ConsoleColor.Green;
    public ConsoleColor WatchingDisabledColor { get; set; } = ConsoleColor.Red;
    public ConsoleColor ProcessIsHealthyColor { get; set; } = ConsoleColor.Green;
    public ConsoleColor ProcessExitedColor { get; set; } = ConsoleColor.White;
    public ConsoleColor ProcessFaultedColor { get; set; } = ConsoleColor.Red;

    public void Visualize(ProcessWatchingStatus status)
    {
        Console.Clear();
        Console.ResetColor();
        Console.WriteLine("(spacebar) start/stop, (esc) quit");

        Console.ForegroundColor = status.IsWatching ? WatchingEnabledColor : WatchingDisabledColor;
        var watchingText = status.IsWatching ? "Watching" : "Not watching";
        var x = GetStartPosition("Not Watching.................");
        Console.SetCursorPosition(x, 4);
        Console.WriteLine(watchingText);

        if (status.ProcessInfo is not null && !string.IsNullOrEmpty(status.ProcessInfo.Name))
        {
            Console.ForegroundColor = status.IsProcessHealthy ? ProcessIsHealthyColor : ProcessFaultedColor;
            var processText = $"{status.ProcessInfo.Name}  {status.ProcessInfo.Id}";
            Console.SetCursorPosition(x, 6);
            Console.WriteLine(processText);

            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(x, 7);
            Console.WriteLine($"memory: {status.ProcessInfo.Memory:N0}");

            Console.SetCursorPosition(x, 8);
            Console.WriteLine($"paged memory: {status.ProcessInfo.PagedMemorySize:N0}");

            Console.SetCursorPosition(x, 9);
            Console.WriteLine($"paged system memory: {status.ProcessInfo.PagedSystemMemorySize:N0}");

            Console.SetCursorPosition(x, 10);
            Console.WriteLine($"private memory: {status.ProcessInfo.PrivateMemorySize:N0}");

            Console.SetCursorPosition(x, 11);
            Console.WriteLine($"processor time: {status.ProcessInfo.TotalProcessorTime}");
        }

        Console.ResetColor();
    }

    private static int GetStartPosition(string text)
    {
        int consoleWidth = Console.WindowWidth;
        int textMiddlePoint = text.Length / 2;
        int consoleMiddlePoint = consoleWidth / 2;

        return consoleMiddlePoint - textMiddlePoint;
    }
}