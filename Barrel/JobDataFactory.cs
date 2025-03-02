using Barrel.Scheduler;

namespace Barrel;

public class JobDataFactory : IJobDataFactory<ScheduledJobData>
{
    public ScheduledJobData Build<T>(ScheduleOptions options) where T : BaseJob, new()
    {
        var jobData = ScheduledJobData.FromJobClass<T>();

        return SetDataToJobData(jobData, options);
    }

    public ScheduledJobData Build(BaseJob job, ScheduleOptions options)
    {
        var jobData = ScheduledJobData.FromJobInstance(job);

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
