namespace Barrel.JobData;

public class ScheduledBaseJobData : BaseJobData
{
    internal ScheduledBaseJobData()
    {
    }

    public DateTime EnqueuedOn { get; internal set; }

    public override DateTime NextScheduleOn() => EnqueuedOn;
}
