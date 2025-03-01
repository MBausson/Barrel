using System.Collections.Concurrent;
using Barrel.Configuration;
using Microsoft.Extensions.Logging;

namespace Barrel.Scheduler;

internal class JobThreadHandler : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly JobSchedulerConfiguration _configuration;

    private readonly ScheduleQueue _scheduleQueue;
    private readonly JobQueue _runningJobQueue;

    //  Queues of job to run. These jobs should be run ASAP, according to their priority
    private readonly ConcurrentDictionary<int, Task> _runningJobs;

    //  The semaphore ensures that we aren't using more threads than we should
    private readonly SemaphoreSlim _semaphore;

    public JobThreadHandler(JobSchedulerConfiguration configuration)
    {
        _configuration = configuration;
        _cancellationTokenSource = new CancellationTokenSource();

        _scheduleQueue = new(_configuration.SchedulePollingRate, _cancellationTokenSource);
        _runningJobQueue = new(_configuration.QueuePollingRate, _configuration.MaxConcurrentJobs,
            _cancellationTokenSource);

        _semaphore = new SemaphoreSlim(_configuration.MaxConcurrentJobs);
        _runningJobs = new ConcurrentDictionary<int, Task>();

        //  Background tasks to handle upcoming jobs
        _scheduleQueue.OnJobReady += JobReady!;
        _runningJobQueue.OnJobFired += JobFired;

        _scheduleQueue.StartProcessSchedules();
        _runningJobQueue.StartProcessJobs();
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
        return _runningJobs.IsEmpty && _runningJobQueue.IsEmpty && _scheduleQueue.IsEmpty;
    }

    private void JobReady(object sender, JobReadyEventArgs eventArgs)
    {
        _runningJobQueue.EnqueueJob(eventArgs.JobData);

        _configuration.Logger.LogDebug($"Enqueued job {eventArgs.JobData.JobId}");
    }

    private void JobFired(object? sender, JobFiredEventArgs e)
    {
        var jobTask = RunJob(e.JobData.InstanceJob!, e.JobData);

        _runningJobs[jobTask.Id] = jobTask;

        //  Removes the job from the running queue after it is completed
        jobTask.ContinueWith(_ => { _runningJobs.Remove(jobTask.Id, out var _); });
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
                _runningJobQueue.EnqueueJob(jobData);

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
