﻿using System.Collections.Concurrent;
using Barrel.Configuration;
using Microsoft.Extensions.Logging;

namespace Barrel.Scheduler;

internal class JobThreadHandler : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly JobSchedulerConfiguration _configuration;

    //  Queues of job to run. These jobs should be run ASAP, according to their priority
    private readonly ConcurrentQueue<ScheduledJobData> _jobQueue;
    private readonly ConcurrentDictionary<int, Task> _runningJobs;
    private readonly SortedList<DateTime, ScheduledJobData> _scheduledJobs;

    //  The semaphore ensures that we aren't using more threads than we should
    private readonly SemaphoreSlim _semaphore;

    public JobThreadHandler(JobSchedulerConfiguration configuration)
    {
        _configuration = configuration;

        _semaphore = new SemaphoreSlim(_configuration.MaxConcurrentJobs);
        _jobQueue = new ConcurrentQueue<ScheduledJobData>();
        _scheduledJobs = new SortedList<DateTime, ScheduledJobData>();
        _runningJobs = new ConcurrentDictionary<int, Task>();
        _cancellationTokenSource = new CancellationTokenSource();

        _ = Task.Run(ProcessQueue);
        _ = Task.Run(ProcessSchedules);
    }

    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        _semaphore.Dispose();
        _cancellationTokenSource.Dispose();

        IsDisposed = true;
        _configuration.Logger.LogDebug($"{nameof(JobThreadHandler)} disposed");
    }

    /// <summary>
    ///     Invoked when any unexpected exception occurs in a running job.
    ///     <remarks>Should also be exposed to public via JobScheduler (TODO)</remarks>
    /// </summary>
    public event EventHandler<JobFailureEventArgs> JobFailure;

    public void ScheduleJob(ScheduledJobData job)
    {
        job.JobState = JobState.Scheduled;

        lock (_scheduledJobs)
        {
            _scheduledJobs.Add(job.EnqueuedOn, job);
        }

        _configuration.Logger.LogInformation($"Scheduled job {job.JobId} to enqueue on {job.EnqueuedOn}");
    }

    public bool AreQueuesEmpty()
    {
        return _runningJobs.IsEmpty && _jobQueue.IsEmpty && _scheduledJobs.Count == 0;
    }

    //  Processes jobs that are still in delay to be enqueued
    private async Task ProcessSchedules()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            KeyValuePair<DateTime, ScheduledJobData>[] jobsToEnqueue;

            lock (_scheduledJobs)
            {
                jobsToEnqueue = _scheduledJobs
                    .Where(kv => kv.Key <= DateTime.Now)
                    .ToArray();
            }

            if (jobsToEnqueue.Length == 0)
            {
                await Task.Delay(_configuration.SchedulePollingRate);
                continue;
            }

            foreach (var (date, job) in jobsToEnqueue)
            {
                _scheduledJobs.Remove(date);
                _jobQueue.Enqueue(job);
                job.JobState = JobState.Enqueued;

                _configuration.Logger.LogDebug($"Enqueued job {job.JobId}");
            }
        }
    }

    //  Processes jobs that have no delay anymore
    //  Complies with the max concurrent jobs limitations
    private async Task ProcessQueue()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            if (!_jobQueue.TryDequeue(out var job))
            {
                await Task.Delay(_configuration.QueuePollingRate);
                continue;
            }

            await _semaphore.WaitAsync();
            job.JobState = JobState.Running;

            var jobInstance = job.HasInstance() ? job.InstanceJob! : job.InstantiateJob();

            var jobTask = Task.Run(async () =>
            {
                try
                {
                    _configuration.Logger.LogDebug($"Launching job {job.JobId} ...");

                    await jobInstance.BeforePerformAsync();
                    await jobInstance.PerformAsync();

                    job.JobState = JobState.Success;

                    _configuration.Logger.LogDebug($"Job {job.JobId} done !");
                }
                catch (Exception e)
                {
                    job.JobState = JobState.Failed;

                    _configuration.Logger.LogError(e, $"Job {job.JobId} failure.");

                    JobFailure.Invoke(job, new JobFailureEventArgs
                    {
                        Job = jobInstance,
                        Exception = e
                    });
                }
                finally
                {
                    _semaphore.Release();
                }
            });

            _runningJobs[jobTask.Id] = jobTask;

            //  Removes the job from the running queue after it is completed
            jobTask.ContinueWith(_ => { _runningJobs.Remove(jobTask.Id, out var _); });
        }
    }
}
