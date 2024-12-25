using Barrel.Configuration;

namespace Barrel;

public class JobScheduler
{
    private readonly JobSchedulerConfiguration _configuration;
    private readonly JobThreadHandler _threadHandler;

    public JobScheduler(JobSchedulerConfigurationBuilder configurationBuilder)
    {
        _configuration = configurationBuilder.Build();
        _threadHandler = new JobThreadHandler(_configuration);
    }

    /// <summary>
    /// Schedules a job to run with no delay.
    /// </summary>
    /// <remarks>This method does not require a job instance, but requires a parameter-less job constructor</remarks>
    /// <typeparam name="T">The <c>BaseJob</c> sub-class implementing the <c>Perform</c> method</typeparam>
    public void Schedule<T>() where T : BaseJob, new() => Schedule(new T(), TimeSpan.Zero);

    /// <summary>
    /// Schedules a job to run with a specified delay.
    /// </summary>
    /// <param name="delay">Delay before invoking the job. The countdown starts on this method's call</param>
    /// <typeparam name="T">The <c>BaseJob</c> subclass implementing the <c>Perform</c> method</typeparam>
    /// <remarks>This method does not require a job instance, but requires a parameter-less job constructor</remarks>
    public void Schedule<T>(TimeSpan delay) where T : BaseJob, new() => Schedule(new T(), delay);

    /// <summary>
    /// Schedules a job to run with no delay.
    /// </summary>
    /// <param name="job">The <c>BaseJob</c> subclass implementing the <c>Perform</c> method</param>
    public void Schedule<T>(T job) where T : BaseJob => Schedule(job, TimeSpan.Zero);

    /// <summary>
    /// Schedules a job to run with a specified delay.
    /// </summary>
    /// <param name="job">The <c>BaseJob</c> sub-class implementing the <c>Perform</c> method</param>
    /// <param name="delay">Delay before invoking the job. The countdown starts on this method's call</param>
    public void Schedule<T>(T job, TimeSpan delay) where T : BaseJob
    {
        _threadHandler.EnqueueJob(job, delay);
    }
}
