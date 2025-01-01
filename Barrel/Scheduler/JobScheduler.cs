using Barrel.Configuration;
using Microsoft.Extensions.Logging;

namespace Barrel.Scheduler;

public class JobScheduler : IDisposable
{
    private readonly JobSchedulerConfiguration _configuration;
    private readonly JobThreadHandler _threadHandler;

    public JobScheduler(JobSchedulerConfigurationBuilder configurationBuilder)
    {
        _configuration = configurationBuilder.Build();
        _threadHandler = new JobThreadHandler(_configuration);

        _threadHandler.JobFailure += OnJobFailure;
    }

    public void Dispose()
    {
        _threadHandler.Dispose();
    }

    /// <summary>
    ///     Schedules a job to run with no delay.
    /// </summary>
    /// <remarks>This method does not require a job instance, but requires a parameter-less job constructor</remarks>
    /// <typeparam name="T">The <c>BaseJob</c> sub-class implementing the <c>Perform</c> method</typeparam>
    public void Schedule<T>() where T : BaseJob, new()
    {
        Schedule(new T(), ScheduleOptions.Default);
    }

    /// <summary>
    ///     Schedules a job to run with a specified delay.
    /// </summary>
    /// <param name="delay">Describes to the Scheduler how should the job be handled (delay, priority...)</param>
    /// <typeparam name="T">The <c>BaseJob</c> subclass implementing the <c>Perform</c> method</typeparam>
    /// <remarks>This method does not require a job instance, but requires a parameter-less job constructor</remarks>
    public void Schedule<T>(ScheduleOptions options) where T : BaseJob, new()
    {
        Schedule(new T(), options);
    }

    /// <summary>
    ///     Schedules a job to run with no delay.
    /// </summary>
    /// <param name="job">The <c>BaseJob</c> subclass implementing the <c>Perform</c> method</param>
    public void Schedule<T>(T job) where T : BaseJob
    {
        Schedule(job, ScheduleOptions.Default);
    }

    /// <summary>
    ///     Schedules a job to run with a specified delay.
    /// </summary>
    /// <param name="job">The <c>BaseJob</c> sub-class implementing the <c>Perform</c> method</param>
    /// <param name="options">Describes to the Scheduler how should the job be handled (delay, priority...)</param>
    public void Schedule<T>(T job, ScheduleOptions options) where T : BaseJob
    {
        _threadHandler.ScheduleJob(job, options.Delay);
    }

    /// <summary>
    ///     Blocking method that waits for all scheduled, enqueued and running jobs to end.
    ///     <remarks>This method does not restrict the schedule of new jobs after it was called</remarks>
    /// </summary>
    public async Task WaitAllJobs()
    {
        await _threadHandler.WaitAllJobs();
    }

    private void OnJobFailure(object _, JobFailureEventArgs eventArgs)
    {
        _configuration.Logger?.LogError(eventArgs.Exception, $"Job {eventArgs.Job.JobId} failure.");
    }
}
