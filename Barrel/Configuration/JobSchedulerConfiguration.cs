namespace Barrel.Configuration;

public struct JobSchedulerConfiguration
{
    public int MaxThreads { get; init; }

    public int QueuePollingRate { get; init; }
}
