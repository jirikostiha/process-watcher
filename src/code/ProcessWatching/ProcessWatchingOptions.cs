using System;

namespace ProcessWatching;

public record ProcessWatchingOptions
{
    /// <summary> Watching process name. </summary>
    public string? ProcessName { get; set; }

    public required string ProcessFile { get; set; }

    public TimeSpan CheckingPeriod { get; set; } = TimeSpan.FromSeconds(5);

    public TimeSpan StartDelay { get; set; } = TimeSpan.FromSeconds(1);
}
