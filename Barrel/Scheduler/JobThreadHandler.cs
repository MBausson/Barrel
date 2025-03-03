using System.Collections.Concurrent;
using Barrel.Configuration;
using Barrel.JobData;
using Barrel.Scheduler.Queues;
using Microsoft.Extensions.Logging;

namespace Barrel.Scheduler;

internal class JobThreadHandler : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly JobSchedulerConfiguration _configuration;
    private readonly JobQueue _runningJobQueue;

    private readonly ConcurrentDictionary<int, Task> _runningJobs;

    private readonly ScheduleQueue _scheduleQueue;

    public JobThreadHandler(JobSchedulerConfiguration configuration)
    {
        _configuration = configuration;
        _cancellationTokenSource = new CancellationTokenSource();

        _scheduleQueue = new ScheduleQueue(_configuration.SchedulePollingRate, _cancellationTokenSource);
        _runningJobQueue = new JobQueue(_configuration.QueuePollingRate, _configuration.MaxConcurrentJobs,
            _cancellationTokenSource);

        _runningJobs = new ConcurrentDictionary<int, Task>();

        _scheduleQueue.OnJobReady += JobReady;
        _runningJobQueue.OnJobFired += JobFired;

        //  Background tasks to handle upcoming jobs
        _scheduleQueue.StartProcessingSchedules();
        _runningJobQueue.StartProcessingJobs();
    }

    public bool IsEmpty => _runningJobs.IsEmpty && _runningJobQueue.IsEmpty && _scheduleQueue.IsEmpty;

    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        _scheduleQueue.OnJobReady -= JobReady;
        _runningJobQueue.OnJobFired -= JobFired;

        _cancellationTokenSource.Dispose();

        IsDisposed = true;
        _configuration.Logger.LogDebug($"{nameof(JobThreadHandler)} disposed");
    }

    public void ScheduleJob(ScheduledBaseJobData baseJobData)
    {
        _scheduleQueue.ScheduleJob(baseJobData);

        _configuration.Logger.LogInformation($"Scheduled job {baseJobData.JobId} to run on {baseJobData.EnqueuedOn}");
    }

    public void ScheduleRecurrentJob(RecurrentBaseJobData baseJobData)
    {
        _scheduleQueue.ScheduleJob(baseJobData);

        _configuration.Logger.LogInformation($"Scheduled recurrent job {baseJobData.JobId}. Next scheduled on {baseJobData.NextScheduleOn()}");
    }

    private void JobReady(object? _, JobReadyEventArgs eventArgs)
    {
        _runningJobQueue.EnqueueJob(eventArgs.BaseJobData);

        _configuration.Logger.LogDebug($"Enqueued job {eventArgs.BaseJobData.JobId}");
    }

    private void JobFired(object? _, JobFiredEventArgs e)
    {
        var jobTask = RunJob(e.BaseJobData.Instance!, e.BaseJobData);

        _runningJobs[jobTask.Id] = jobTask;

        //  Removes the job from the running queue after it is completed
        jobTask.ContinueWith(_ => { _runningJobs.Remove(jobTask.Id, out var _); });
    }

    private async Task RunJob(BaseJob jobInstance, ScheduledBaseJobData baseJobData)
    {
        try
        {
            _configuration.Logger.LogDebug($"Launching job {baseJobData.JobId} ...");

            //  Job execution
            await jobInstance.BeforePerformAsync();
            await jobInstance.PerformAsync();

            baseJobData.JobState = JobState.Success;

            _configuration.Logger.LogDebug($"Job {baseJobData.JobId} done !");

            RescheduleIfRecurrent(baseJobData);
        }
        catch (Exception e)
        {
            HandleFailingJob(baseJobData, e);
        }
        finally
        {
            _runningJobQueue.JobFinished();
        }
    }

    private void HandleFailingJob(ScheduledBaseJobData baseJobData, Exception e)
    {
        _configuration.Logger.LogError(e, $"Job {baseJobData.JobId} failure.");

        baseJobData.JobState = JobState.Failed;

        if (baseJobData.ShouldRetry)
        {
            baseJobData.Retry();

            baseJobData.JobState = JobState.Enqueued;
            _runningJobQueue.EnqueueJob(baseJobData);

            _configuration.Logger.LogDebug(
                $"Retrying job {baseJobData.JobId} ({baseJobData.RetryAttempts}/{baseJobData.MaxRetryAttempts}) ...");
        }
        else RescheduleIfRecurrent(baseJobData);

        //  For recurrent failing jobs, we re-schedule when the job cannot be retried
    }

    private void RescheduleIfRecurrent(ScheduledBaseJobData baseJobData)
    {
        if (baseJobData is RecurrentBaseJobData recurrentJobData)
        {
            recurrentJobData.EnqueuedOn = recurrentJobData.NextScheduleOn();
            _scheduleQueue.ScheduleJob(recurrentJobData);

            _configuration.Logger.LogInformation($"Rescheduling reccurent job {baseJobData.JobId} to run on {baseJobData.EnqueuedOn}");
        }
    }
}
