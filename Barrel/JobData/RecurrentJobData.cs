﻿using Barrel.Scheduler;

namespace Barrel.JobData;

//  TODO: Remove the inheritance from ScheduledJobData
//  Make so both jobdata classes implement an interface / abstract class
//  giving the next schedule date, job id, etc...
public class RecurrentJobData : ScheduledJobData
{
    public DateTime NextScheduleOn() => Options.NextScheduleOn();

    public required RecurrentScheduleOptions Options { get; init; }
}
