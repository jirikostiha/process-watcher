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
        Console.SetCursorPosition(GetStartPosition(watchingText), 4);
        Console.WriteLine(watchingText);

        Console.ForegroundColor = status.IsProcessHealthy ? ProcessIsHealthyColor : ProcessFaultedColor;
        var processText = $"{status.ProcessInfo?.Id} {status.ProcessInfo?.Name}";
        Console.SetCursorPosition(GetStartPosition(processText), 5);
        Console.WriteLine(processText);

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