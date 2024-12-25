using Barrel.Configuration;

namespace Barrel;

public class JobSchedulerConfigurationBuilder
{
    public int MaxThreads { get; private set; } = 5;
    public int QueuePollingRate { get; private set; } = 100;

    public JobSchedulerConfigurationBuilder WithMaxThreads(int maxThreads)
    {
        if (maxThreads < 1) throw new ArgumentException("MaxThread property must be greater than zero");

        MaxThreads = maxThreads;

        return this;
    }

    public JobSchedulerConfigurationBuilder WithQueuePollingRate(int milliseconds)
    {
        if (milliseconds < 1) throw new ArgumentException("QueuePollingRate property must be greater than zero");

        QueuePollingRate = milliseconds;

        return this;
    }

    public JobSchedulerConfiguration Build() => new()
    {
        MaxThreads = MaxThreads,
        QueuePollingRate = QueuePollingRate
    };
}
