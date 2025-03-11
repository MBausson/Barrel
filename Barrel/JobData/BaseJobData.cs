namespace Barrel.JobData;

public abstract class BaseJobData
{
    /// <summary>
    ///     A unique identifier bound to the creation of a job.
    ///     <remarks>This field will be overridable in the future</remarks>
    /// </summary>
    public Guid JobId { get; } = Guid.NewGuid();

    /// <summary>
    ///     The priority level of a job, which will influence the scheduler
    /// </summary>
    internal JobPriority JobPriority { get; set; }

    /// <summary>
    ///     The current state of a job. This value is updated by the scheduler.
    /// </summary>
    public JobState JobState { get; internal set; }

    public int MaxRetryAttempts { get; internal set; }

    public int RetryAttempts { get; internal set; }

    public bool ShouldRetry => MaxRetryAttempts > RetryAttempts;

    public Type? JobClass { get; internal set; }

    public BaseJob? Instance { get; internal set; }

    public abstract DateTime NextScheduleOn();

    //  If not already done, instantiate the job's class, stores it and returns it
    public BaseJob InstantiateJob()
    {
        if (JobClass is null || Instance is not null) return Instance!;

        Instance = (BaseJob)Activator.CreateInstance(JobClass)!;

        return Instance;
    }

    public bool HasInstance() => Instance is not null;

    internal void Retry()
    {
        RetryAttempts++;
        Instance = null;
    }
}
