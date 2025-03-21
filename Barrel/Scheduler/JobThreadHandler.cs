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

    private readonly JobQueue _jobQueue;
    private readonly ScheduleQueue _scheduleQueue;

    private readonly ConcurrentDictionary<int, Task> _runningJobs;

    public JobThreadHandler(JobSchedulerConfiguration configuration)
    {
        _configuration = configuration;
        _cancellationTokenSource = new CancellationTokenSource();

        _scheduleQueue = new ScheduleQueue(_configuration.SchedulePollingRate, _cancellationTokenSource);
        _jobQueue = new JobQueue(_configuration.QueuePollingRate, _configuration.MaxConcurrentJobs,
            _cancellationTokenSource);

        _runningJobs = new ConcurrentDictionary<int, Task>();

        _scheduleQueue.OnJobReady += JobReady;
        _jobQueue.OnJobFired += JobFired;

        //  Background tasks to handle upcoming jobs
        _scheduleQueue.StartProcessingSchedules();
        _jobQueue.StartProcessingJobs();
    }

    public bool IsEmpty()
    {
        Console.WriteLine($"RJ: {_runningJobs.IsEmpty} JQ: {_jobQueue.IsEmpty} SQ: {_scheduleQueue.IsEmpty}");
        return _runningJobs.IsEmpty && _jobQueue.IsEmpty && _scheduleQueue.IsEmpty;
    }

    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        _scheduleQueue.OnJobReady -= JobReady;
        _jobQueue.OnJobFired -= JobFired;

        _cancellationTokenSource.Dispose();

        IsDisposed = true;
        _configuration.Logger.LogDebug($"{nameof(JobThreadHandler)} disposed");
    }

    public void ScheduleJob(ScheduledBaseJobData baseJobData)
    {
        _scheduleQueue.ScheduleJob(baseJobData);

        _configuration.Logger.LogInformation($"Scheduled job {baseJobData.JobId} to run on {baseJobData.EnqueuedOn}");
    }

    public void ScheduleRecurrentJob(RecurrentJobData jobData)
    {
        jobData.EnqueuedOn = jobData.NextScheduleOn();
        _scheduleQueue.ScheduleJob(jobData);

        _configuration.Logger.LogInformation($"Scheduled recurrent job {jobData.JobId}. Next scheduled on {jobData.NextScheduleOn()}");
    }

    private void JobReady(object? _, JobReadyEventArgs eventArgs)
    {
        _jobQueue.EnqueueJob(eventArgs.JobData);

        _configuration.Logger.LogDebug($"Enqueued job {eventArgs.JobData.JobId}");
    }

    private void JobFired(object? _, JobFiredEventArgs e)
    {
        var jobTask = RunJob(e.BaseJobData.Instance!, e.BaseJobData);

        _runningJobs[jobTask.Id] = jobTask;

        //  Removes the job from the running queue after it is completed
        jobTask.ContinueWith(_ => { _runningJobs.Remove(jobTask.Id, out var _); });
    }

    private async Task RunJob(BaseJob jobInstance, BaseJobData jobData)
    {
        try
        {
            _configuration.Logger.LogDebug($"Launching job {jobData.JobId} ...");

            //  Job execution
            await jobInstance.BeforePerformAsync();
            await jobInstance.PerformAsync();

            jobData.JobState = JobState.Success;

            _configuration.Logger.LogDebug($"Job {jobData.JobId} done !");

            RescheduleIfRecurrent(jobData);
        }
        catch (Exception e)
        {
            HandleFailingJob(jobData, e);
        }
        finally
        {
            _jobQueue.JobFinished();
        }
    }

    private void HandleFailingJob(BaseJobData jobData, Exception e)
    {
        _configuration.Logger.LogError(e, $"Job {jobData.JobId} failure.");

        jobData.JobState = JobState.Failed;

        if (jobData.ShouldRetry)
        {
            jobData.Retry();

            jobData.JobState = JobState.Enqueued;
            _jobQueue.EnqueueJob(jobData);

            _configuration.Logger.LogDebug(
                $"Retrying job {jobData.JobId} ({jobData.RetryAttempts}/{jobData.MaxRetryAttempts}) ...");
        }
        else RescheduleIfRecurrent(jobData);

        //  For recurrent failing jobs, we re-schedule when the job cannot be retried
    }

    private void RescheduleIfRecurrent(BaseJobData jobData)
    {
        if (jobData is RecurrentJobData recurrentJobData)
        {
            if (!recurrentJobData.HasNextSchedule())
            {
                _configuration.Logger.LogDebug($"Recurrent job {jobData.JobId} has no next schedule");
                return;
            }

            recurrentJobData.EnqueuedOn = recurrentJobData.NextScheduleOn();
            recurrentJobData.Instance = null;

            _scheduleQueue.ScheduleJob(recurrentJobData);

            _configuration.Logger.LogInformation($"Rescheduling reccurent job {jobData.JobId} to run on {recurrentJobData.EnqueuedOn}");
        }
    }
}
