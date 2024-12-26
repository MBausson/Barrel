namespace Barrel;

public abstract class BaseJob
{
    /// <summary>
    /// The priority level of a job, which will influence the scheduler
    /// </summary>
    public virtual JobPriority JobPriority { get; } = JobPriority.Medium;

    /// <summary>
    /// The current state of a job. This value is updated by the scheduler.
    /// </summary>
    public JobState JobState { get; internal set; } = JobState.NotStarted;

    /// <summary>
    /// A unique identifier bound to the creation of a job.
    /// <remarks>This field will be overridable in the future</remarks>
    /// </summary>
    public readonly Guid JobId = Guid.NewGuid();

    /// <summary>
    /// Hook method called before the execution of a job.
    /// </summary>
    protected internal virtual void BeforeSchedule() { }

    /// <summary>
    /// Method called during the execution of a job.
    /// </summary>
    protected internal abstract Task PerformAsync();
}
