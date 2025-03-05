using Barrel.Scheduler;

namespace Barrel.JobData.Factory;

public class RecurrentJobDataFactory : IJobDataFactory<RecurrentJobData, RecurrentScheduleOptions>
{
    public RecurrentJobData Create<TJob>(RecurrentScheduleOptions options) where TJob : BaseJob, new()
    {
        return new RecurrentJobData
        {
            JobClass = typeof(TJob),
            Options = options
        };
    }

    public RecurrentJobData Create(BaseJob job, RecurrentScheduleOptions options)
    {
        throw new InvalidOperationException($"Cannot use instanced jobs for recurrent jobs.");
    }
}
