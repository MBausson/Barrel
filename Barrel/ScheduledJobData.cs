using Barrel.Scheduler;

namespace Barrel;

public class ScheduledJobData
{
    private Type? _jobClass;

    private ScheduledJobData()
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

    public BaseJob? InstanceJob { get; private set; }

    public int MaxRetryAttempts { get; internal set; }

    public int RetryAttempts { get; private set; }

    internal bool ShouldRetry => MaxRetryAttempts > RetryAttempts;

    public static ScheduledJobData FromJobInstance(BaseJob jobInstance)
    {
        return new ScheduledJobData
        {
            InstanceJob = jobInstance
        };
    }

    public static ScheduledJobData FromJobClass<T>() where T : BaseJob, new()
    {
        return new ScheduledJobData
        {
            _jobClass = typeof(T)
        };
    }

    //  If not already done, instantiate the job's class, stores it and returns it
    public BaseJob InstantiateJob()
    {
        if (_jobClass is null || InstanceJob is not null) return InstanceJob!;

        InstanceJob = (BaseJob)Activator.CreateInstance(_jobClass)!;

        return InstanceJob;
    }

    public bool HasInstance()
    {
        return InstanceJob is not null;
    }

    internal void Retry()
    {
        RetryAttempts++;
        InstanceJob = null;
    }
}
