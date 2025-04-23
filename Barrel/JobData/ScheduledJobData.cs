namespace Barrel.JobData;

public class ScheduledJobData : BaseJobData
{
    internal ScheduledJobData()
    {
    }

    public DateTimeOffset EnqueuedOn { get; internal set; }

    public override DateTimeOffset NextScheduleOn()
    {
        return EnqueuedOn;
    }

    public virtual bool HasNextSchedule()
    {
        return true;
    }
}