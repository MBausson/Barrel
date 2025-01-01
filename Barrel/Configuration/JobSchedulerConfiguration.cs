using Microsoft.Extensions.Logging;

namespace Barrel.Configuration;

public struct JobSchedulerConfiguration
{
    /// <summary>
    ///     Represents the maximum amount of jobs that can run concurrently for a given Scheduler
    /// </summary>
    public int MaxConcurrentJobs { get; init; }

    /// <summary>
    ///     Represents the amount of delay after no enqueued jobs have been found to be executed.
    ///     If this value is too high, some jobs might execute with additional delay.
    ///     Setting this value too low will deteriorate CPU performance
    /// </summary>
    public int QueuePollingRate { get; init; }

    /// <summary>
    ///     Represents the amount of delay after no scheduled jobs have been found to be enqueued.
    ///     If this value is too high, some jobs might execute with additional delay.
    ///     Setting this value too low will deteriorate CPU performance
    /// </summary>
    public int SchedulePollingRate { get; init; }

    /// <summary>
    /// Logger used by the scheduler instance.
    /// <seealso cref="ILogger"/>
    /// </summary>
    public ILogger? Logger { get; init; }
}
