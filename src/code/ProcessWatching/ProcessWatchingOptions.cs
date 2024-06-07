using System;

namespace ProcessWatching;

/// <summary>
/// Process watching policy.
/// </summary>
public record ProcessWatchingOptions
{
    public TimeSpan CheckingPeriod { get; set; } = TimeSpan.FromSeconds(5);
}