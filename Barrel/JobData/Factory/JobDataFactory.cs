using Barrel.Scheduler;

namespace Barrel.JobData.Factory;

public class JobDataFactory : IJobDataFactory<ScheduledJobData, ScheduleOptions>
{
    public ScheduledJobData Build<TJob>(ScheduleOptions options) where TJob : BaseJob, new()
    {
        var jobData = new ScheduledJobData
        {
            JobClass = typeof(TJob)
        };

        return SetDataToJobData(jobData, options);
    }

    public ScheduledJobData Build(BaseJob job, ScheduleOptions options)
    {
        var jobData = new ScheduledJobData
        {
            Instance = job
        };

        return SetDataToJobData(jobData, options);
    }

    private ScheduledJobData SetDataToJobData(ScheduledJobData jobData, ScheduleOptions options)
    {
        jobData.JobPriority = options.Priority;
        jobData.EnqueuedOn = options.NextScheduleOn();
        jobData.MaxRetryAttempts = options.MaxRetries;

        return jobData;
    }
}
