using Barrel.Scheduler;

namespace Barrel.JobData.Factory;

public class JobDataFactory : IJobDataFactory<ScheduledBaseJobData, ScheduleOptions>
{
    public IReadOnlyList<ScheduledBaseJobData> CreateFromCalendar<TJob>(CalendarScheduleOptions options) where TJob : BaseJob, new()
    {
        var jobsData = new List<ScheduledBaseJobData>();

        foreach (var scheduleDateTime in options.ScheduleDateTimes)
        {
            var jobData = new ScheduledBaseJobData
            {
                JobClass = typeof(TJob)
            };

            jobData.JobPriority = options.Priority;
            jobData.EnqueuedOn = scheduleDateTime;
            jobData.MaxRetryAttempts = options.MaxRetries;

            jobsData.Add(jobData);
        }

        return jobsData;
    }

    public ScheduledBaseJobData Create<TJob>(ScheduleOptions options) where TJob : BaseJob, new()
    {
        var jobData = new ScheduledBaseJobData
        {
            JobClass = typeof(TJob)
        };

        return SetDataToJobData(jobData, options);
    }

    public ScheduledBaseJobData Create(BaseJob job, ScheduleOptions options)
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
