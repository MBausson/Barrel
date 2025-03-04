﻿using Barrel.Configuration;
using Barrel.JobData;
using Barrel.JobData.Factory;
using Microsoft.Extensions.Logging;

namespace Barrel.Scheduler;

public class JobScheduler : IDisposable
{
    private readonly JobSchedulerConfiguration _configuration;
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

    public RecurrentJobData ScheduleRecurrent<T>(RecurrentScheduleOptions options) where T : BaseJob, new()
    {
        var jobData = new RecurrentJobDataFactory().Create<T>(options);

        _threadHandler.ScheduleRecurrentJob(jobData);

        return jobData;
    }

    //  TODO: Return a CalendarJobData instead. This will indicate future and past executions
    public IEnumerable<ScheduledBaseJobData> ScheduleCalendar<T>(CalendarScheduleOptions options) where T : BaseJob, new()
    {
        if (options.ScheduleDateTimes.Count == 0) throw new InvalidOperationException($"No date have been specified for this schedule.");

        var jobsData = new JobDataFactory().CreateFromCalendar<T>(options);
        foreach (var jobData in jobsData) _threadHandler.ScheduleJob(jobData);

        return jobsData;
    }

    /// <summary>
    ///     Schedules a job to run with no delay.
    /// </summary>
    /// <remarks>This method does not require a job instance, but requires a parameter-less job constructor</remarks>
    /// <typeparam name="T">The <c>BaseJob</c> sub-class implementing the <c>Perform</c> method</typeparam>
    public ScheduledBaseJobData Schedule<T>() where T : BaseJob, new()
    {
        return Schedule(new T(), ScheduleOptions.Default);
    }

    /// <summary>
    ///     Schedules a job to run with no delay.
    /// </summary>
    /// <param name="job">The <c>BaseJob</c> subclass implementing the <c>Perform</c> method</param>
    public ScheduledBaseJobData Schedule<T>(T job) where T : BaseJob
    {
        return Schedule(job, ScheduleOptions.Default);
    }

    /// <summary>
    ///     Schedules a job to run with a specified delay.
    /// </summary>
    /// <param name="job">The <c>BaseJob</c> sub-class implementing the <c>Perform</c> method</param>
    /// <param name="options">Describes to the Scheduler how should the job be handled (delay, priority...)</param>
    public ScheduledBaseJobData Schedule<T>(T job, ScheduleOptions options) where T : BaseJob
    {
        var jobData = new JobDataFactory().Create(job, options);

        return ScheduleFromJobData(jobData);
    }

    /// <summary>
    ///     Schedules a job to run with a specified delay.
    /// </summary>
    /// <param name="delay">Describes to the Scheduler how should the job be handled (delay, priority...)</param>
    /// <typeparam name="T">The <c>BaseJob</c> subclass implementing the <c>Perform</c> method</typeparam>
    /// <remarks>This method does not require a job instance, but requires a parameter-less job constructor</remarks>
    public ScheduledBaseJobData Schedule<T>(ScheduleOptions options) where T : BaseJob, new()
    {
        var jobData = new JobDataFactory().Create<T>(options);

        return ScheduleFromJobData(jobData);
    }

    private ScheduledBaseJobData ScheduleFromJobData(ScheduledBaseJobData baseJobData)
    {
        _threadHandler.ScheduleJob(baseJobData);

        return baseJobData;
    }

    /// <summary>
    ///     Blocking method that waits for all scheduled, enqueued and running jobs to end.
    ///     <remarks>This method does not restrict the schedule of new jobs after it was called</remarks>
    /// </summary>
    public async Task WaitAllJobs()
    {
        while (!_threadHandler.IsDisposed && !_threadHandler.IsEmpty) await Task.Delay(50);
    }
}
