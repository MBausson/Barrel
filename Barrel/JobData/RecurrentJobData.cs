using Barrel.Scheduler;

namespace Barrel.JobData;

//  TODO: Remove the inheritance from ScheduledJobData
//  Make so both jobdata classes implement an interface / abstract class
//  giving the next schedule date, job id, etc...
public class RecurrentJobData : ScheduledBaseJobData
{
    /// <summary>
    /// Called when a recurrent job has been fired.
    /// TODO: Remove when not needed anymore
    /// </summary>
    public virtual void OnRecurrentJobFired() { }

    public new virtual DateTime NextScheduleOn() => Options.NextScheduleOn();

    public override bool HasNextSchedule() => true;

    public required RecurrentScheduleOptions Options { get; init; }
}
