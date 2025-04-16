using Barrel.Configuration;
using Barrel.Exceptions;
using Barrel.JobData;
using Barrel.JobData.Factory;
using Barrel.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Barrel.Scheduler;

public class JobScheduler : IDisposable
{
    private readonly JobSchedulerConfiguration _configuration;
    private readonly Dictionary<Type, bool> _instantiableCache = [];
    private readonly JobThreadHandler _threadHandler;

    public JobScheduler() : this(new JobSchedulerConfigurationBuilder())
    {
    }

    public JobScheduler(JobSchedulerConfigurationBuilder configurationBuilder)
    {
        _configuration = configurationBuilder.Build();
        _threadHandler = new JobThreadHandler(_configuration);

        _configuration.Logger.LogDebug("JobScheduler initialized");
    }

    public void Dispose()
    {
        _threadHandler.Dispose();

        _configuration.Logger.LogDebug($"{nameof(JobScheduler)} disposed");
    }

    /// <summary>
    ///     Schedule a recurrent job to be executed every X delay
    /// </summary>
    /// <remarks>The job must be instantiated either via an argument-less constructor or via dependency injection</remarks>
    /// <typeparam name="TJob">The <c>BaseJob</c> subclass implementing the <c>Perform</c> method</typeparam>
    public RecurrentJobData ScheduleRecurrent<TJob>(RecurrentScheduleOptions options) where TJob : BaseJob, new()
    {
        EnsureJobInstantiation<TJob>();

        var jobData = new JobDataFactory().Create<RecurrentJobData, TJob, RecurrentScheduleOptions>(options);
        _threadHandler.ScheduleRecurrentJob(jobData);

        return jobData;
    }

    /// <summary>
    ///     Schedule a recurrent job to be on a precise date
    /// </summary>
    /// <remarks>The job must be instantiated either via an argument-less constructor or via dependency injection</remarks>
    /// <typeparam name="TJob">The <c>BaseJob</c> subclass implementing the <c>Perform</c> method</typeparam>
    public IEnumerable<ScheduledJobData> ScheduleCalendar<TJob>(CalendarScheduleOptions options)
        where TJob : BaseJob, new()
    {
        if (options.ScheduleDateTimes.Count == 0)
            throw new InvalidOperationException("No date have been specified for this schedule.");

        EnsureJobInstantiation<TJob>();

        var calendarJobData = new JobDataFactory().Create<CalendarJobData, TJob, CalendarScheduleOptions>(options);
        foreach (var jobData in calendarJobData.ScheduledJobs) _threadHandler.ScheduleJob(jobData);

        return calendarJobData.ScheduledJobs;
    }

    /// <summary>
    ///     Schedules a job to run with no delay.
    /// </summary>
    /// <typeparam name="TJob">The <c>BaseJob</c> sub-class implementing the <c>Perform</c> method</typeparam>
    public ScheduledJobData Schedule<TJob>() where TJob : BaseJob
    {
        return Schedule<TJob>(ScheduleOptions.Default);
    }

    /// <summary>
    ///     Schedules a job to run with no delay.
    /// </summary>
    /// <param name="job">The <c>BaseJob</c> subclass implementing the <c>Perform</c> method</param>
    public ScheduledJobData Schedule<TJob>(TJob job) where TJob : BaseJob
    {
        return Schedule(job, ScheduleOptions.Default);
    }

    /// <summary>
    /// Schedules an anonymous job to run without delay.
    /// </summary>
    /// <param name="actionJob">The action the job will be running</param>
    public ScheduledJobData Schedule(Action actionJob) => Schedule(new AnonymousJob(actionJob));

    /// <summary>
    ///     Schedules a job to run with specified options.
    /// </summary>
    /// <param name="job">The <c>BaseJob</c> subclass implementing the <c>Perform</c> method</param>
    /// <param name="options">Describes to the Scheduler how should the job be handled (delay, priority...)</param>
    public ScheduledJobData Schedule<TJob>(TJob job, ScheduleOptions options) where TJob : BaseJob
    {
        var jobData = new JobDataFactory().Create<ScheduledJobData, ScheduleOptions>(job, options);

        return ScheduleFromJobData(jobData);
    }

    /// <summary>
    /// Schedules an anonymous job to run with specified options.
    /// </summary>
    /// <param name="actionJob">The action the job will be running</param>
    /// <param name="options">Describes to the Scheduler how should the job be handled (delay, priority...)</param>
    public ScheduledJobData Schedule(Action actionJob, ScheduleOptions options) =>
        Schedule(new AnonymousJob(actionJob), options);

    /// <summary>
    ///     Schedules a job to run with a specified delay.
    /// </summary>
    /// <param name="delay">Describes to the Scheduler how should the job be handled (delay, priority...)</param>
    /// <typeparam name="TJob">The <c>BaseJob</c> subclass implementing the <c>Perform</c> method</typeparam>
    public ScheduledJobData Schedule<TJob>(ScheduleOptions options) where TJob : BaseJob
    {
        EnsureJobInstantiation<TJob>();
        var jobData = new JobDataFactory().Create<ScheduledJobData, TJob, ScheduleOptions>(options);

        return ScheduleFromJobData(jobData);
    }

    /// <summary>
    ///     Blocking method that waits for all scheduled, enqueued and running jobs to end.
    ///     <remarks>This method does not restrict the schedule of new jobs after it was called</remarks>
    /// </summary>
    public async Task WaitAllJobs()
    {
        while (!_threadHandler.IsDisposed && !_threadHandler.IsEmpty()) await Task.Delay(50);
    }

    private ScheduledJobData ScheduleFromJobData(ScheduledJobData jobData)
    {
        _threadHandler.ScheduleJob(jobData);

        return jobData;
    }

    /// <summary>
    ///     Ensures that a job class can be instantiated via DI or an argument-less constructor.
    ///     <exception cref="Exceptions.ImpossibleJobInstantiation">Thrown when a job cannot be instantiated.</exception>
    /// </summary>
    private void EnsureJobInstantiation<TJob>() where TJob : BaseJob
    {
        var type = typeof(TJob);

        if (_instantiableCache.TryGetValue(type, out var cachedValue))
        {
            if (!cachedValue) throw new ImpossibleJobInstantiation<TJob>();

            return;
        }

        if (ArgumentLessConstructorChecker.HasArgumentLessConstructor(typeof(TJob)))
        {
            _instantiableCache[type] = true;
            return;
        }

        //  No argument-less constructor here, but DI might help
        if (_configuration.ServiceProvider is not null)
            //  TODO: Maybe add a configuration value to pre-instantiate (or no) the job ?
            //  Pre-instantiating might have side-effects
            if (_configuration.ServiceProvider.GetService<TJob>() is not null)
            {
                _instantiableCache[type] = true;
                return;
            }

        //  No argument-less constructor, no DI => impossible
        _instantiableCache[type] = false;
        throw new ImpossibleJobInstantiation<TJob>();
    }
}
