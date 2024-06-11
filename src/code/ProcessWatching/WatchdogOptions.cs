using System;
using System.IO;

namespace ProcessWatching;

public record WatchdogOptions
{
    /// <summary>
    /// Process name.
    /// </summary>
    public string ProcessName => Path.GetFileNameWithoutExtension(ProcessFile);

    /// <summary>
    /// File to execute.
    /// </summary>
    public required string ProcessFile { get; set; }

    /// <summary>
    /// Delay before process start.
    /// </summary>
    public TimeSpan StartDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// After this duration from start the process is considered as successfully up and running.
    /// </summary>
    public TimeSpan StartWindow { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Start delay is multiplied by this coefficient every unsuccessful start.
    /// </summary>
    public double DelayCoef { get; set; } = 1.3;
}