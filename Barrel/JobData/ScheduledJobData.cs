namespace Barrel.JobData;

public class ScheduledJobData
{
    internal ScheduledJobData()
    {
    }

    /// <summary>
    ///     A unique identifier bound to the creation of a job.
    ///     <remarks>This field will be overridable in the future</remarks>
    /// </summary>
    public Guid JobId { get; } = Guid.NewGuid();

    /// <summary>
    ///     The priority level of a job, which will influence the scheduler
    /// </summary>
    public JobPriority JobPriority { get; internal set; } = JobPriority.Medium;

    /// <summary>
    ///     The current state of a job. This value is updated by the scheduler.
    /// </summary>
    public JobState JobState { get; internal set; } = JobState.NotStarted;

    public DateTime EnqueuedOn { get; internal set; }

    public BaseJob? Instance { get; internal set; }

    public Type? JobClass { get; internal set; }

    public int MaxRetryAttempts { get; internal set; }

    public int RetryAttempts { get; private set; }

    internal bool ShouldRetry => MaxRetryAttempts > RetryAttempts;

    //  If not already done, instantiate the job's class, stores it and returns it
    public BaseJob InstantiateJob()
    {
        if (JobClass is null || Instance is not null) return Instance!;

        Instance = (BaseJob)Activator.CreateInstance(JobClass)!;

        return Instance;
    }

    public bool HasInstance()
    {
        return Instance is not null;
    }

    internal void Retry()
    {
        RetryAttempts++;
        Instance = null;
    }
}
