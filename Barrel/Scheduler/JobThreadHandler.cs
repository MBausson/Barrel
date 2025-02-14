using System.Collections.Concurrent;
using Barrel.Configuration;
using Microsoft.Extensions.Logging;

namespace Barrel.Scheduler;

internal class JobThreadHandler : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly JobSchedulerConfiguration _configuration;
    private readonly ScheduleQueue _scheduleQueue;

    //  Queues of job to run. These jobs should be run ASAP, according to their priority
    private readonly ConcurrentQueue<ScheduledJobData> _jobQueue;
    private readonly ConcurrentDictionary<int, Task> _runningJobs;

    //  The semaphore ensures that we aren't using more threads than we should
    private readonly SemaphoreSlim _semaphore;

    public JobThreadHandler(JobSchedulerConfiguration configuration)
    {
        _configuration = configuration;
        _cancellationTokenSource = new CancellationTokenSource();

        _scheduleQueue = new(_configuration.SchedulePollingRate, _cancellationTokenSource);

        _semaphore = new SemaphoreSlim(_configuration.MaxConcurrentJobs);
        _jobQueue = new ConcurrentQueue<ScheduledJobData>();
        _runningJobs = new ConcurrentDictionary<int, Task>();

        //  Background tasks to handle upcoming jobs
        _scheduleQueue.OnJobReady += JobReady!;
        _scheduleQueue.StartProcessSchedules();

        //  This one handles running jobs
        _ = Task.Run(ProcessQueue);
    }

    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        _scheduleQueue.OnJobReady -= JobReady!;

        _semaphore.Dispose();
        _cancellationTokenSource.Dispose();

        IsDisposed = true;
        _configuration.Logger.LogDebug($"{nameof(JobThreadHandler)} disposed");
    }

    public void ScheduleJob(ScheduledJobData jobData)
    {
        _scheduleQueue.ScheduleJob(jobData);

        _configuration.Logger.LogInformation($"Scheduled job {jobData.JobId} to enqueue on {jobData.EnqueuedOn}");
    }

    public bool AreQueuesEmpty()
    {
        return _runningJobs.IsEmpty && _jobQueue.IsEmpty && _scheduleQueue.IsEmpty;
    }

    private void JobReady(object sender, JobReadyEventArgs eventArgs)
    {
        _jobQueue.Enqueue(eventArgs.JobData);
        eventArgs.JobData.JobState = JobState.Enqueued;

        _configuration.Logger.LogDebug($"Enqueued job {eventArgs.JobData.JobId}");
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
