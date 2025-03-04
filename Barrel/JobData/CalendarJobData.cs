namespace Barrel.JobData;

public class CalendarJobData : RecurrentJobData
{
    public required Queue<DateTime> SchedulesDatetime { get; init; } = new();

    public override void OnRecurrentJobFired()
    {
        SchedulesDatetime.Dequeue();
    }

    public override DateTime EnqueuedOn => SchedulesDatetime.Peek();

    public override DateTime NextScheduleOn() => SchedulesDatetime.Peek();

    public override bool HasNextSchedule() => SchedulesDatetime.Count != 0;
}
