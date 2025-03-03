using Barrel.Scheduler;

namespace Barrel.JobData.Factory;

public class RecurrentJobDataFactory : IJobDataFactory<RecurrentBaseJobData, RecurrentScheduleOptions>
{
    public RecurrentBaseJobData Build<TJob>(RecurrentScheduleOptions options) where TJob : BaseJob, new()
    {
        var jobData = new RecurrentBaseJobData
        {
            JobClass = typeof(TJob),
            Options = options
        };

        return jobData;
    }

    public RecurrentBaseJobData Build(BaseJob job, RecurrentScheduleOptions options)
    {
        var jobData = new RecurrentBaseJobData
        {
            Options = options,
            Instance = job
        };

        return jobData;
    }
}
