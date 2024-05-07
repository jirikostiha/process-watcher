namespace ProcessWatching.ConsoleApp;

public record ProcessWatchingStatus
{
    public bool IsWatching { get; set; }

    public bool IsProcessHealthy { get; set; }

    public required string ProcessFile { get; set; }

    public ProcessInfo ProcessInfo { get; set; } = new ProcessInfo();
}