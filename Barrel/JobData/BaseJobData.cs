namespace Barrel.JobData;

public abstract class BaseJobData
{
    /// <summary>
    ///     A unique identifier bound to the creation of a job.
    ///     <remarks>This field will be overridable in the future</remarks>
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    ///     The priority level of a job, which will influence the scheduler
    /// </summary>
    public JobPriority Priority { get; internal set; }

    /// <summary>
    ///     The current state of a job. This value is updated by the scheduler.
    /// </summary>
    public JobState State { get; internal set; }

    public int MaxRetryAttempts { get; internal set; }

    public int RetryAttempts { get; internal set; }

    public bool ShouldRetry => ShouldRetryIndefinitely || MaxRetryAttempts > RetryAttempts;

    public Type JobClass { get; internal set; }

    public BaseJob? Instance { get; internal set; }

    public bool ShouldRetryIndefinitely => MaxRetryAttempts == -1;

    public bool IsCancellable => State switch
    {
        JobState.Scheduled or JobState.Enqueued => true,
        var _ => false
    };

    /// <summary>
    /// Indicates wether or not a job is stopped
    /// <remarks>Failed and cancelled jobs are considered as stopped</remarks>
    /// </summary>
    public bool IsStopped => State is JobState.Cancelled or JobState.Failed or JobState.Success;

    public abstract DateTimeOffset NextScheduleOn();

    internal bool HasInstance()
    {
        return Instance is not null;
    }

    internal void Retry()
    {
        RetryAttempts++;
        Instance = null;
    }
}
