using System;
using System.Collections.Generic;
using System.IO;

namespace ProcessWatching.ConsoleApp;

public static class IOExtensions
{
    public static IEnumerable<DirectoryInfo> GetParents(this DirectoryInfo directory, int depth = int.MaxValue)
    {
        ArgumentNullException.ThrowIfNull(directory);
        ArgumentOutOfRangeException.ThrowIfLessThan(depth, 0);

        var current = directory;
        int currentDepth = 0;

        while (current.Parent != null && currentDepth < depth)
        {
            current = current.Parent;
            yield return current;
            currentDepth++;
        }
    }

    public static IEnumerable<string> GetParents(this string directory, int depth = int.MaxValue)
    {
        ArgumentException.ThrowIfNullOrEmpty(directory);
        ArgumentOutOfRangeException.ThrowIfLessThan(depth, 0);

        var current = directory;
        int currentDepth = 0;

        while (!string.IsNullOrEmpty(current) && currentDepth < depth)
        {
            var parent = Path.GetDirectoryName(current);
            if (string.IsNullOrEmpty(parent))
                break;

            yield return parent;
            current = parent;
            currentDepth++;
        }
    }
}