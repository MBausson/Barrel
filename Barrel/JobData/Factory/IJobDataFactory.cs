using Barrel.Scheduler;

namespace Barrel.JobData.Factory;

public interface IJobDataFactory
{
    public TJobData Create<TJobData, TJob, TOptions>(TOptions options)
        where TJobData : ScheduledJobData
        where TJob : BaseJob
        where TOptions : ScheduleOptions;

    public TJobData Create<TJobData, TOptions>(BaseJob job, TOptions options)
        where TJobData : ScheduledJobData
        where TOptions : ScheduleOptions;
}
