using Barrel.Scheduler;

namespace Barrel.JobData.Factory;

public class RecurrentJobDataFactory : IJobDataFactory<RecurrentJobData, RecurrentScheduleOptions>
{
    public RecurrentJobData Build<TJob>(RecurrentScheduleOptions options) where TJob : BaseJob, new()
    {
        var jobData = new RecurrentJobData
        {
            JobClass = typeof(TJob),
            Options = options
        };

        return jobData;
    }

    public RecurrentJobData Build(BaseJob job, RecurrentScheduleOptions options)
    {
        var jobData = new RecurrentJobData
        {
            Options = options,
            Instance = job
        };

        return jobData;
    }
}
