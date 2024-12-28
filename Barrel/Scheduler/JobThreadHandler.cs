using System.Collections.Concurrent;
using Barrel.Configuration;

namespace Barrel.Scheduler;

internal class JobThreadHandler : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource;

    private readonly JobSchedulerConfiguration _configuration;

    //  Queues of job to run. These jobs should be run ASAP, according to their priority
    private readonly ConcurrentQueue<BaseJob> _jobQueue;
    private readonly ConcurrentDictionary<int, Task> _runningJobs;

    private readonly SortedList<DateTime, BaseJob> _scheduledJobs;

    //  The semaphore ensures that we aren't using more threads than we should
    private readonly SemaphoreSlim _semaphore;

    public JobThreadHandler(JobSchedulerConfiguration configuration)
    {
        _configuration = configuration;

        _semaphore = new SemaphoreSlim(_configuration.MaxThreads);
        _jobQueue = new ConcurrentQueue<BaseJob>();
        _scheduledJobs = new SortedList<DateTime, BaseJob>();
        _runningJobs = new ConcurrentDictionary<int, Task>();
        _cancellationTokenSource = new CancellationTokenSource();

        _ = Task.Run(ProcessQueue);
        _ = Task.Run(ProcessSchedules);
    }

    public void Dispose()
    {
        _semaphore.Dispose();
        _cancellationTokenSource.Dispose();
    }

    public void ScheduleJob(BaseJob job, TimeSpan delay)
    {
        job.JobState = JobState.Scheduled;

        lock (_scheduledJobs)
        {
            _scheduledJobs.Add(DateTime.Now + delay, job);
        }
    }

    /// <summary>
    ///     Blocking method that waits for all scheduled, enqueued and running jobs to end.
    /// </summary>
    public async Task WaitAllJobs()
    {
        while (!_runningJobs.IsEmpty || !_jobQueue.IsEmpty || _scheduledJobs.Count != 0) await Task.Delay(50);
    }

    //  Processes jobs that are still in delay to be enqueued
    private async Task ProcessSchedules()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            KeyValuePair<DateTime, BaseJob>[] jobsToEnqueue;

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
            }
        }
    }

    //  Processes jobs that have no delay anymore
    //  Complies with the thread pool size limitations
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

            var jobTask = Task.Run(async () =>
            {
                try
                {
                    await job.BeforePerformAsync();
                    await job.PerformAsync();
                    job.JobState = JobState.Success;
                }
                catch (Exception)
                {
                    //  TODO: Rethrow the error to the main thread.
                    job.JobState = JobState.Failed;
                    throw;
                }
                finally
                {
                    _semaphore.Release();
                }
            });

            _runningJobs[jobTask.Id] = jobTask;

            jobTask.ContinueWith(_ => { _runningJobs.Remove(jobTask.Id, out var _); });
        }
    }
}