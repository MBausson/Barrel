namespace Barrel;

public class ScheduledJobData
{
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

    private Type? JobClass;

    private ScheduledJobData(){}

    public static ScheduledJobData FromJobInstance(BaseJob jobInstance)
    {
        return new()
        {
            InstanceJob = jobInstance,
        };
    }

    public static ScheduledJobData FromJobClass<T>() where T : BaseJob, new()
    {
        return new ()
        {
            JobClass = typeof(T)
        };
    }

    public BaseJob InstantiateJob()
    {
        if (JobClass is null) return InstanceJob!;

        InstanceJob = (BaseJob)Activator.CreateInstance(JobClass)!;

        return InstanceJob;
    }

    public bool HasInstance() => InstanceJob is not null;
}
