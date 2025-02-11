using System.Collections.Concurrent;
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

        //  Background tasks to handle upcoming jobs

        //  This one handles waiting (queuing) jobs
        _ = Task.Run(ProcessSchedules);

        //  This one handles running jobs
        _ = Task.Run(ProcessQueue);
    }

    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        _semaphore.Dispose();
        _cancellationTokenSource.Dispose();

        IsDisposed = true;
        _configuration.Logger.LogDebug($"{nameof(JobThreadHandler)} disposed");
    }

    public void ScheduleJob(ScheduledJobData jobData)
    {
        jobData.JobState = JobState.Scheduled;

        lock (_scheduledJobs)
        {
            _scheduledJobs.Add(jobData.EnqueuedOn, jobData);
        }

        _configuration.Logger.LogInformation($"Scheduled job {jobData.JobId} to enqueue on {jobData.EnqueuedOn}");
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

            foreach (var (date, jobData) in jobsToEnqueue)
            {
                _scheduledJobs.Remove(date);
                _jobQueue.Enqueue(jobData);
                jobData.JobState = JobState.Enqueued;

                _configuration.Logger.LogDebug($"Enqueued job {jobData.JobId}");
            }
        }
    }

    //  Processes jobs that have no delay anymore
    //  Complies with the max concurrent jobs limitations
    private async Task ProcessQueue()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            if (!_jobQueue.TryDequeue(out var jobData))
            {
                await Task.Delay(_configuration.QueuePollingRate);
                continue;
            }

            await _semaphore.WaitAsync();
            jobData.JobState = JobState.Running;

            //  If the job hasn't been instantiated, do it now
            var jobInstance = jobData.HasInstance() ? jobData.InstanceJob! : jobData.InstantiateJob();
            var jobTask = RunJob(jobInstance, jobData);

            _runningJobs[jobTask.Id] = jobTask;

            //  Removes the job from the running queue after it is completed
            jobTask.ContinueWith(_ => { _runningJobs.Remove(jobTask.Id, out var _); });
        }
    }

    private async Task RunJob(BaseJob jobInstance, ScheduledJobData jobData)
    {
        try
        {
            _configuration.Logger.LogDebug($"Launching job {jobData.JobId} ...");

            await jobInstance.BeforePerformAsync();
            await jobInstance.PerformAsync();

            jobData.JobState = JobState.Success;

            _configuration.Logger.LogDebug($"Job {jobData.JobId} done !");
        }
        catch (Exception e)
        {
            _configuration.Logger.LogError(e, $"Job {jobData.JobId} failure.");

            jobData.JobState = JobState.Failed;

            if (jobData.ShouldRetry)
            {
                jobData.Retry();

                jobData.JobState = JobState.Enqueued;
                _jobQueue.Enqueue(jobData);

                _configuration.Logger.LogDebug(
                    $"Retrying job {jobData.JobId} ({jobData.RetryAttempts}/{jobData.MaxRetryAttempts}) ...");
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
