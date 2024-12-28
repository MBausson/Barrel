﻿namespace Barrel.Configuration;

public class JobSchedulerConfigurationBuilder
{
    /// <inheritdoc cref="WithMaxThreads"/>
    public int MaxThreads { get; private set; } = 5;
    /// <inheritdoc cref="WithQueuePollingRate"/>
    public int QueuePollingRate { get; private set; } = 100;
    /// <inheritdoc cref="WithSchedulePollingRate"/>
    public int SchedulePollingRate { get; private set; } = 100;

    /// <inheritdoc cref="JobSchedulerConfiguration.MaxThreads" />
    public JobSchedulerConfigurationBuilder WithMaxThreads(int maxThreads)
    {
        if (maxThreads < 1) throw new ArgumentException("MaxThread property must be greater than zero");

        MaxThreads = maxThreads;

        return this;
    }

    /// <inheritdoc cref="JobSchedulerConfiguration.QueuePollingRate" />
    public JobSchedulerConfigurationBuilder WithQueuePollingRate(int milliseconds)
    {
        if (milliseconds < 0) throw new ArgumentException("QueuePollingRate property must be positive");

        QueuePollingRate = milliseconds;

        return this;
    }

    /// <inheritdoc cref="JobSchedulerConfiguration.SchedulePollingRate" />
    public JobSchedulerConfigurationBuilder WithSchedulePollingRate(int milliseconds)
    {
        if (milliseconds < 0) throw new ArgumentException("SchedulePollingRate property must be positive");

        SchedulePollingRate = milliseconds;

        return this;
    }

    public JobSchedulerConfiguration Build()
    {
        return new JobSchedulerConfiguration
        {
            MaxThreads = MaxThreads,
            QueuePollingRate = QueuePollingRate
        };
    }
}
