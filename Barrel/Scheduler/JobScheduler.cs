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

    public RecurrentJobData ScheduleRecurrent<TJob>(RecurrentScheduleOptions options) where TJob : BaseJob, new()
    {
        EnsureJobInstantiation<TJob>();

        var jobData = new JobDataFactory().Create<RecurrentJobData, TJob, RecurrentScheduleOptions>(options);
        _threadHandler.ScheduleRecurrentJob(jobData);

        return jobData;
    }

    public IEnumerable<ScheduledJobData> ScheduleCalendar<TJob>(CalendarScheduleOptions options) where TJob : BaseJob, new()
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
    /// <remarks>This method does not require a job instance, but requires a parameter-less job constructor</remarks>
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
    ///     Schedules a job to run with a specified delay.
    /// </summary>
    /// <param name="job">The <c>BaseJob</c> sub-class implementing the <c>Perform</c> method</param>
    /// <param name="options">Describes to the Scheduler how should the job be handled (delay, priority...)</param>
    public ScheduledJobData Schedule<TJob>(TJob job, ScheduleOptions options) where TJob : BaseJob
    {
        var jobData = new JobDataFactory().Create<ScheduledJobData, ScheduleOptions>(job, options);

        return ScheduleFromJobData(jobData);
    }

    /// <summary>
    ///     Schedules a job to run with a specified delay.
    /// </summary>
    /// <param name="delay">Describes to the Scheduler how should the job be handled (delay, priority...)</param>
    /// <typeparam name="TJob">The <c>BaseJob</c> subclass implementing the <c>Perform</c> method</typeparam>
    /// <remarks>This method does not require a job instance, but requires a parameter-less job constructor</remarks>
    public ScheduledJobData Schedule<TJob>(ScheduleOptions options) where TJob : BaseJob
    {
        EnsureJobInstantiation<TJob>();
        var jobData = new JobDataFactory().Create<ScheduledJobData, TJob, ScheduleOptions>(options);

        return ScheduleFromJobData(jobData);
    }

    private ScheduledJobData ScheduleFromJobData(ScheduledJobData jobData)
    {
        _threadHandler.ScheduleJob(jobData);

        return jobData;
    }

    /// <summary>
    ///     Blocking method that waits for all scheduled, enqueued and running jobs to end.
    ///     <remarks>This method does not restrict the schedule of new jobs after it was called</remarks>
    /// </summary>
    public async Task WaitAllJobs()
    {
        while (!_threadHandler.IsDisposed && !_threadHandler.IsEmpty()) await Task.Delay(50);
    }

    /// <summary>
    /// Ensures that a job class can be instantiated via DI or an argument-less constructor.
    /// <exception cref="Exceptions.ImpossibleJobInstantiation">Thrown when a job cannot be instantiated.</exception>
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
        {
            //  TODO: Maybe add a configuration value to pre-instantiate (or no) the job ?
            //  Pre-instantiating might have side-effects
            if (_configuration.ServiceProvider.GetService<TJob>() is not null)
            {
                _instantiableCache[type] = true;
                return;
            }
        }

        //  No argument-less constructor, no DI => impossible
        _instantiableCache[type] = false;
        throw new ImpossibleJobInstantiation<TJob>();
    }
}
