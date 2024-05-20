using System;

namespace ProcessWatching;

public record ProcessInfo
{
    public int? Id { get; set; }

    public string? Name { get; set; }

    public long Memory { get; set; } //WorkingSet64

    public TimeSpan TotalProcessorTime { get; set; }

    public long PagedMemorySize { get; set; }

    public long PagedSystemMemorySize { get; set; }

    public long PrivateMemorySize { get; set; }
}