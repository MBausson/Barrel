using Barrel.Scheduler;

namespace Barrel.JobData.Factory;

public class RecurrentJobDataFactory : IJobDataFactory<RecurrentJobData, RecurrentScheduleOptions>
{
    public RecurrentJobData Build<TJob>(RecurrentScheduleOptions options) where TJob : BaseJob, new()
    {
        if (options is CalendarScheduleOptions calendarOptions)
            return new CalendarJobData
            {
                JobClass = typeof(TJob),
                Options = options,
                SchedulesDatetime = calendarOptions.ToSchedulesQueue(),
            };

        return new RecurrentJobData
        {
            JobClass = typeof(TJob),
            Options = options
        };
    }

    public RecurrentJobData Build(BaseJob job, RecurrentScheduleOptions options)
    {
        throw new InvalidOperationException($"Cannot use instanced jobs for recurrent jobs.");
    }
}
