using Barrel.Exceptions;
using Barrel.Scheduler;

namespace Barrel.JobData.Factory;

public class JobDataFactory : IJobDataFactory
{
    public TJobData Create<TJobData, TJob, TOptions>(TOptions options)
        where TJobData : ScheduledJobData
        where TJob : BaseJob
        where TOptions : ScheduleOptions
    {
        return options switch
        {
            CalendarScheduleOptions calendarOptions =>
                (TJobData)(ScheduledJobData)CreateCalendarJobData<TJob>(calendarOptions),

            RecurrentScheduleOptions recurrentOptions =>
                (TJobData)(ScheduledJobData)CreateRecurrentJobData<TJob>(recurrentOptions),

            _ => (TJobData)CreateScheduledJobData(typeof(TJob), options)
        };
    }

    //  Factories that receive instances of job cannot handle calendar nor recurrent jobs.
    //  It does not make sense to keep alive a BaseJob instance for multiple job executions.
    public TJobData Create<TJobData, TOptions>(BaseJob job, TOptions options) where TJobData : ScheduledJobData
        where TOptions : ScheduleOptions
    {
        if (options is CalendarScheduleOptions or RecurrentScheduleOptions)
            throw new ImpossibleJobInstantiation<BaseJob>();

        var jobData = new ScheduledJobData
        {
            JobClass = job.GetType(),
            Instance = job
        };

        return (TJobData)SetDataToJobData(jobData, options);
    }

    private CalendarJobData CreateCalendarJobData<TJob>(CalendarScheduleOptions options) where TJob : BaseJob
    {
        var scheduledJobs = options.ScheduleDateTimes.Select(dateTime => new ScheduledJobData
        {
            JobClass = typeof(TJob),
            EnqueuedOn = dateTime,
            JobPriority = options.Priority,
            MaxRetryAttempts = options.MaxRetries
        }).ToArray();

        return new CalendarJobData
        {
            ScheduledJobs = scheduledJobs,
            JobPriority = options.Priority,
            MaxRetryAttempts = options.MaxRetries
        };
    }

    private RecurrentJobData CreateRecurrentJobData<TJob>(RecurrentScheduleOptions options) where TJob : BaseJob
    {
        return new RecurrentJobData
        {
            JobClass = typeof(TJob),
            Options = options
        };
    }

    private ScheduledJobData CreateScheduledJobData(Type jobType, ScheduleOptions options)
    {
        return new ScheduledJobData
        {
            JobClass = jobType,
            EnqueuedOn = options.NextScheduleOn(),
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
