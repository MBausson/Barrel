namespace Barrel;

public abstract class BaseJob
{
    public virtual JobPriority JobPriority { get; } = JobPriority.Medium;

    public JobState JobState { get; internal set; } = JobState.NotStarted;

    public readonly Guid JobId = Guid.NewGuid();

    protected internal virtual void BeforeSchedule() { }

    protected internal abstract void Perform();
}
