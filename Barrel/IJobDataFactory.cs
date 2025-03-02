using Barrel.Scheduler;

namespace Barrel;

public interface IJobDataFactory<T> where T : ScheduledJobData
{
    public T Build<TJob>(ScheduleOptions options) where TJob : BaseJob, new();

    public T Build(BaseJob job, ScheduleOptions options);
}
