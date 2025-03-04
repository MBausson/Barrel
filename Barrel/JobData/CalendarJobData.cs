namespace Barrel.JobData;

public class CalendarJobData : ScheduledBaseJobData
{
    public required Queue<DateTime> FutureJobsDateTimes { get; init; } = new();

    public override DateTime EnqueuedOn => FutureJobsDateTimes.Peek();

    public override DateTime NextScheduleOn() => FutureJobsDateTimes.Peek();

    public override bool HasNextSchedule() => FutureJobsDateTimes.Count != 0;
}
