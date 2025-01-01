using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Barrel.Configuration;

public class JobSchedulerConfigurationBuilder
{
    /// <inheritdoc cref="JobSchedulerConfiguration.MaxConcurrentJobs"/>
    public int MaxConcurrentJobs { get; private set; } = 5;
    /// <inheritdoc cref="JobSchedulerConfiguration.QueuePollingRate"/>
    public int QueuePollingRate { get; private set; } = 100;
    /// <inheritdoc cref="JobSchedulerConfiguration.SchedulePollingRate"/>
    public int SchedulePollingRate { get; private set; } = 100;

    /// <summary>
    /// <inheritdoc cref="JobSchedulerConfiguration.Logger"/>
    /// <remarks>By default, logs to stdout.</remarks>
    /// </summary>
    public ILogger? Logger { get; private set; } = DefaultLogger();

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

    /// <inheritdoc cref="JobSchedulerConfiguration.Logger"/>
    /// <param name="builder">The builder action used by ILoggerFactory</param>
    public JobSchedulerConfigurationBuilder WithLogger(Action<ILoggingBuilder> builder)
    {
        using ILoggerFactory factory = LoggerFactory.Create(builder);

        Logger = factory.CreateLogger("Barrel");

        return this;
    }

    /// <summary>
    /// Specifies no logging
    /// </summary>
    public JobSchedulerConfigurationBuilder WithNoLogger()
    {
        Logger = null;

        return this;
    }

    public JobSchedulerConfiguration Build()
    {
        return new JobSchedulerConfiguration
        {
            MaxConcurrentJobs = MaxConcurrentJobs,
            QueuePollingRate = QueuePollingRate,
            Logger = Logger
        };
    }

    private static ILogger DefaultLogger()
    {
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());

        return factory.CreateLogger("Barrel");
    }
}
