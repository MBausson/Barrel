namespace Barrel.JobData;

public class CalendarJobData : ScheduledBaseJobData
{
    public required ScheduledBaseJobData[] ScheduledJobs { get; init; }
}
