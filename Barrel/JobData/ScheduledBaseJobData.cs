namespace Barrel.JobData;

public class ScheduledBaseJobData : BaseJobData
{
    internal ScheduledBaseJobData()
    {
    }

    public virtual DateTime EnqueuedOn { get; internal set; }

    public override DateTime NextScheduleOn() => EnqueuedOn;

    public virtual bool HasNextSchedule() => true;
}
