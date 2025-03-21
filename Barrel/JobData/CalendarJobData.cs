namespace Barrel.JobData;

public class CalendarJobData : ScheduledJobData
{
    public required ScheduledJobData[] ScheduledJobs { get; init; }
}