using System;

namespace ProcessWatching;

public class ProcessEventArgs : EventArgs
{
    public ProcessEventArgs(ProcessInfo processInfo, string? message = null)
    {
        ProcessInfo = processInfo;
        Message = message;
    }

    public ProcessInfo ProcessInfo { get; set; }

    public string? Message { get; set; }
}