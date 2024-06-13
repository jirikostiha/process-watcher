using Spectre.Console;

namespace ProcessWatching.ConsoleApp;

public class SpectreVisualizer : IVisualizer
{
    private static readonly Markup _choiceText =
        new("[gray](spacebar)[/] [white]start/stop[/]   [gray](escape)[/] [white]quit[/]");
    private static readonly FigletText _watchingText = new FigletText("Watching").Centered();
    private static readonly FigletText _onText = new FigletText("On").Color(Color.Green).Centered();
    private static readonly FigletText _offText = new FigletText("Off").Color(Color.Grey).Centered();

    public void Visualize(ProcessWatchingStatus status)
    {
        AnsiConsole.Clear();

        AnsiConsole.Write(_watchingText);
        if (status.IsWatching)
            AnsiConsole.Write(_onText);
        else
            AnsiConsole.Write(_offText);

        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine();

        if (status.ProcessInfo is not null && !string.IsNullOrEmpty(status.ProcessInfo.Name))
        {
            var table = new Table().HideHeaders().NoBorder();
            table.AddColumn("").RightAligned();
            table.AddColumn("").RightAligned();

            if (status.IsProcessHealthy)
                table.AddRow("Name", $"[green]{status.ProcessInfo.Name}[/]");
            else
                table.AddRow("Name", $"[red]{status.ProcessInfo.Name}[/]");

            table.AddRow("Id", $"[white]{status.ProcessInfo.Id}[/]");
            table.AddRow("Memory", $"[white]{status.ProcessInfo.Memory:N0}[/]");
            table.AddRow("Paged memory", $"[white]{status.ProcessInfo.PagedMemorySize:N0}[/]");
            table.AddRow("Paged system memory", $"[white]{status.ProcessInfo.PagedSystemMemorySize:N0}[/]");
            table.AddRow("private memory", $"[white]{status.ProcessInfo.PrivateMemorySize:N0}[/]");
            table.AddRow("processor time", $"[white]{status.ProcessInfo.TotalProcessorTime}[/]");
            table.AddRow("running time", $"[white]{status.ProcessInfo.RunningTime}[/]");

            AnsiConsole.Write(table.Centered());
        }

        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine();

        AnsiConsole.Write(_choiceText.Centered());
    }
}