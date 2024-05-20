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

        if (status.ProcessInfo is not null && !string.IsNullOrEmpty(status.ProcessInfo.Name))
        {
            Console.ForegroundColor = status.IsProcessHealthy ? ProcessIsHealthyColor : ProcessFaultedColor;
            var xposition = GetStartPosition(status.ProcessInfo.Name);
            var processText = $"{status.ProcessInfo.Name}  {status.ProcessInfo.Id}";
            Console.SetCursorPosition(xposition, 6);
            Console.WriteLine(processText);

            if (status.ProcessInfo.Memory > 0)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(xposition, 7);
                Console.WriteLine($"memory: {status.ProcessInfo.Memory:N0}");
            }
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