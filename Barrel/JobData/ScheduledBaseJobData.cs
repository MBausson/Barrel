namespace Barrel.JobData;

public class ScheduledBaseJobData : BaseJobData
{
    internal ScheduledBaseJobData()
    {
    }

    public DateTime EnqueuedOn { get; internal set; }

    internal bool ShouldRetry => MaxRetryAttempts > RetryAttempts;

    public override DateTime NextScheduleOn() => EnqueuedOn;
}
