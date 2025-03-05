namespace Barrel.JobData.Factory;

public interface IJobDataFactory<out T, TOptions> where T : ScheduledBaseJobData
{
    public T Create<TJob>(TOptions options) where TJob : BaseJob, new();

    public T Create(BaseJob job, TOptions options);
}
