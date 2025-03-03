namespace Barrel.JobData.Factory;

public interface IJobDataFactory<out T, TOptions> where T : ScheduledBaseJobData
{
    public T Build<TJob>(TOptions options) where TJob : BaseJob, new();

    public T Build(BaseJob job, TOptions options);
}
