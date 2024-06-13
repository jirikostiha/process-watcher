namespace ProcessWatching.ConsoleApp;

public interface IVisualizer
{
    void Visualize(ProcessWatchingStatus status);
}