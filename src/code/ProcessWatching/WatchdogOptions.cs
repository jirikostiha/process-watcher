using System;

namespace ProcessWatching;

public record WatchdogOptions
{
    public required string ProcessName { get; set; }

    public required string ProcessFile { get; set; }

    public TimeSpan StartDelay { get; set; } = TimeSpan.FromSeconds(1);
}
