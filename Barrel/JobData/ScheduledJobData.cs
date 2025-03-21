namespace Barrel.JobData;

public class ScheduledJobData : BaseJobData
{
    internal ScheduledJobData()
    {
    }

    public virtual DateTime EnqueuedOn { get; internal set; }

    public override DateTime NextScheduleOn() => EnqueuedOn;

    public virtual bool HasNextSchedule() => true;
}
