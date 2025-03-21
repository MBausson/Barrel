using Barrel.Scheduler;

namespace Barrel.JobData.Factory;

public class JobDataFactory : IJobDataFactory
{
    public TJobData Create<TJobData, TJob, TOptions>(TOptions options)
        where TJobData : ScheduledJobData
        where TJob : BaseJob, new()
        where TOptions : ScheduleOptions
    {
        if (options is CalendarScheduleOptions calendarScheduleOptions)
        {
            return (TJobData)(ScheduledJobData)CreateFromCalendar<TJob>(calendarScheduleOptions);
        }

        if (options is RecurrentScheduleOptions recurrentScheduleOptions)
        {
            return (TJobData)(ScheduledJobData)new RecurrentJobData
            {
                JobClass = typeof(TJob),
                Options = recurrentScheduleOptions
            };
        }

        var jobData = new ScheduledJobData
        {
            JobClass = typeof(TJob)
        };

        return (TJobData)SetDataToJobData(jobData, options);
    }

    //  Factories that receive instances of job cannot handle calendar nor recurrent jobs.
    //  It does not make sense to keep alive a BaseJob instance for multiple job executions.
    public TJobData Create<TJobData, TOptions>(BaseJob job, TOptions options) where TJobData : ScheduledJobData where TOptions : ScheduleOptions
    {
        if (options is CalendarScheduleOptions or RecurrentScheduleOptions)
            throw new InvalidOperationException("Cannot use instanced jobs for recurrent jobs.");

        var jobData = new ScheduledJobData
        {
            Instance = job
        };

        return (TJobData)SetDataToJobData(jobData, options);
    }

    private CalendarJobData CreateFromCalendar<TJob>(CalendarScheduleOptions options) where TJob : BaseJob, new()
    {
        var jobsData = new List<ScheduledJobData>();

        foreach (var scheduleDateTime in options.ScheduleDateTimes)
        {
            var data = new ScheduledJobData
            {
                JobClass = typeof(TJob)
            };

            data.JobPriority = options.Priority;
            data.EnqueuedOn = scheduleDateTime;
            data.MaxRetryAttempts = options.MaxRetries;

            jobsData.Add(data);
        }

        return new CalendarJobData
        {
            ScheduledJobs = jobsData.ToArray(),
            JobPriority = options.Priority,
            MaxRetryAttempts = options.MaxRetries
        };
    }

    private ScheduledJobData SetDataToJobData(ScheduledJobData jobData, ScheduleOptions options)
    {
        jobData.JobPriority = options.Priority;
        jobData.EnqueuedOn = options.NextScheduleOn();
        jobData.MaxRetryAttempts = options.MaxRetries;

        return jobData;
    }
}
