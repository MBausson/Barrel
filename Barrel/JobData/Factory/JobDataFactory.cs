using Barrel.Scheduler;

namespace Barrel.JobData.Factory;

public class JobDataFactory : IJobDataFactory<ScheduledBaseJobData, ScheduleOptions>
{
    public ScheduledBaseJobData Build<TJob>(ScheduleOptions options) where TJob : BaseJob, new()
    {
        var jobData = new ScheduledBaseJobData
        {
            JobClass = typeof(TJob)
        };

        return SetDataToJobData(jobData, options);
    }

    public ScheduledBaseJobData Build(BaseJob job, ScheduleOptions options)
    {
        var jobData = new ScheduledBaseJobData
        {
            Instance = job
        };

        return SetDataToJobData(jobData, options);
    }

    private ScheduledBaseJobData SetDataToJobData(ScheduledBaseJobData baseJobData, ScheduleOptions options)
    {
        baseJobData.JobPriority = options.Priority;
        baseJobData.EnqueuedOn = options.NextScheduleOn();
        baseJobData.MaxRetryAttempts = options.MaxRetries;

        return baseJobData;
    }
}
