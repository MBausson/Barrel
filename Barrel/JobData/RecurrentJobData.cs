using Barrel.Scheduler;

namespace Barrel.JobData;

//  TODO: Remove the inheritance from ScheduledJobData
//  Make so both jobdata classes implement an interface / abstract class
//  giving the next schedule date, job id, etc...
public class RecurrentJobData : ScheduledJobData
{
    public required RecurrentScheduleOptions Options { get; init; }

    public new virtual DateTimeOffset NextScheduleOn()
    {
        return Options.NextScheduleOn();
    }

    public override bool HasNextSchedule()
    {
        return true;
    }
}