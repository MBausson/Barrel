using System.Collections.Concurrent;
using Barrel.Configuration;
using Microsoft.Extensions.Logging;

namespace Barrel.Scheduler;

internal class JobThreadHandler : IDisposable
{
    public bool IsEmpty => _runningJobs.IsEmpty && _runningJobQueue.IsEmpty && _scheduleQueue.IsEmpty;

    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly JobSchedulerConfiguration _configuration;

    private readonly ScheduleQueue _scheduleQueue;
    private readonly JobQueue _runningJobQueue;

    private readonly ConcurrentDictionary<int, Task> _runningJobs;

    public JobThreadHandler(JobSchedulerConfiguration configuration)
    {
        _configuration = configuration;
        _cancellationTokenSource = new CancellationTokenSource();

        _scheduleQueue = new(_configuration.SchedulePollingRate, _cancellationTokenSource);
        _runningJobQueue = new(_configuration.QueuePollingRate, _configuration.MaxConcurrentJobs,
            _cancellationTokenSource);

        _runningJobs = new ConcurrentDictionary<int, Task>();

        _scheduleQueue.OnJobReady += JobReady;
        _runningJobQueue.OnJobFired += JobFired;

        //  Background tasks to handle upcoming jobs
        _scheduleQueue.StartProcessingSchedules();
        _runningJobQueue.StartProcessingJobs();
    }

    public bool IsDisposed { get; private set; }

    public void ScheduleJob(ScheduledJobData jobData)
    {
        _scheduleQueue.ScheduleJob(jobData);

        _configuration.Logger.LogInformation($"Scheduled job {jobData.JobId} to enqueue on {jobData.EnqueuedOn}");
    }

    public void Dispose()
    {
        _scheduleQueue.OnJobReady -= JobReady;
        _runningJobQueue.OnJobFired -= JobFired;

        _cancellationTokenSource.Dispose();

        IsDisposed = true;
        _configuration.Logger.LogDebug($"{nameof(JobThreadHandler)} disposed");
    }

    private void JobReady(object? _, JobReadyEventArgs eventArgs)
    {
        _runningJobQueue.EnqueueJob(eventArgs.JobData);

        _configuration.Logger.LogDebug($"Enqueued job {eventArgs.JobData.JobId}");
    }

    private void JobFired(object? _, JobFiredEventArgs e)
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
            HandleFailingJob(jobData, e);
        }
        finally
        {
            _runningJobQueue.JobFinished();
        }
    }

    private void HandleFailingJob(ScheduledJobData jobData, Exception e)
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
}
