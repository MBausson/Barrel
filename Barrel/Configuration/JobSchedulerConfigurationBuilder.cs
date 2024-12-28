namespace Barrel.Configuration;

public class JobSchedulerConfigurationBuilder
{
    /// <inheritdoc cref="JobSchedulerConfiguration.MaxConcurrentJobs"/>
    public int MaxConcurrentJobs { get; private set; } = 5;
    /// <inheritdoc cref="JobSchedulerConfiguration.QueuePollingRate"/>
    public int QueuePollingRate { get; private set; } = 100;
    /// <inheritdoc cref="JobSchedulerConfiguration.SchedulePollingRate"/>
    public int SchedulePollingRate { get; private set; } = 100;

    /// <inheritdoc cref="JobSchedulerConfiguration.MaxConcurrentJobs" />
    public JobSchedulerConfigurationBuilder WithMaxConcurrentJobs(int maxConcurrentJobs)
    {
        if (maxConcurrentJobs < 1) throw new ArgumentException($"{nameof(MaxConcurrentJobs)} property must be greater than zero");

        MaxConcurrentJobs = maxConcurrentJobs;

        return this;
    }

    /// <inheritdoc cref="JobSchedulerConfiguration.QueuePollingRate" />
    public JobSchedulerConfigurationBuilder WithQueuePollingRate(int milliseconds)
    {
        if (milliseconds < 0) throw new ArgumentException($"{nameof(QueuePollingRate)} property must be positive");

        QueuePollingRate = milliseconds;

        return this;
    }

    /// <inheritdoc cref="JobSchedulerConfiguration.SchedulePollingRate" />
    public JobSchedulerConfigurationBuilder WithSchedulePollingRate(int milliseconds)
    {
        if (milliseconds < 0) throw new ArgumentException($"{nameof(SchedulePollingRate)} property must be positive");

        SchedulePollingRate = milliseconds;

        return this;
    }

    public JobSchedulerConfiguration Build()
    {
        return new JobSchedulerConfiguration
        {
            MaxConcurrentJobs = MaxConcurrentJobs,
            QueuePollingRate = QueuePollingRate
        };
    }
}
