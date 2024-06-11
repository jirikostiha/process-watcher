using System;
using System.Diagnostics;

namespace ProcessWatching;

public static class ProcessExtensions
{
    public static ProcessInfo GetProcessInfo(this Process? process, string? defaultName = null)
    {
        if (process is null)
        {
            return new ProcessInfo()
            {
                Name = defaultName
            };
        }
        else if (process.HasExited)
        {
            return new ProcessInfo()
            {
                Name = defaultName
            };
        }
        else
        {
            return new ProcessInfo()
            {
                Name = process.ProcessName,
                Id = process.Id,
                Memory = process.WorkingSet64,
                TotalProcessorTime = process.TotalProcessorTime,
                RunningTime = TimeProvider.System.GetUtcNow() - process.StartTime.ToUniversalTime(),
                PagedMemorySize = process.PagedMemorySize64,
                PagedSystemMemorySize = process.PagedSystemMemorySize64,
                PrivateMemorySize = process.PrivateMemorySize64,
            };
        }
    }
}