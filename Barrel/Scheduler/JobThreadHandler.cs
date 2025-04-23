using System.Collections.Concurrent;
using Barrel.Configuration;
using Barrel.JobData;
using Barrel.Scheduler.Queues;
using Barrel.Utils;
using Microsoft.Extensions.Logging;

namespace Barrel.Scheduler;

internal class JobThreadHandler : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly JobSchedulerConfiguration _configuration;
    private readonly ConcurrentDictionary<int, RunningJob> _runningJobs;

    private readonly ScheduleQueue _scheduleQueue;
    private readonly WaitQueue _waitQueue;

    public JobThreadHandler(JobSchedulerConfiguration configuration)
    {
        _configuration = configuration;
        _cancellationTokenSource = new CancellationTokenSource();

        _scheduleQueue = new ScheduleQueue(_configuration.SchedulePollingRate, _cancellationTokenSource);
        _waitQueue = new WaitQueue(_configuration.QueuePollingRate, _configuration.MaxConcurrentJobs,
            _cancellationTokenSource);

        _runningJobs = new ConcurrentDictionary<int, RunningJob>();

        _scheduleQueue.OnJobReady += JobReady;
        _waitQueue.OnJobFired += JobStarted;

        //  Background tasks to handle upcoming jobs
        _scheduleQueue.StartProcessingSchedules();
        _waitQueue.StartProcessingJobs();
    }

    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        _scheduleQueue.OnJobReady -= JobReady;
        _waitQueue.OnJobFired -= JobStarted;

        _cancellationTokenSource.Dispose();

        IsDisposed = true;
        _configuration.Logger.LogDebug($"{nameof(JobThreadHandler)} disposed");
    }

    public bool IsEmpty()
    {
        return _runningJobs.IsEmpty && _waitQueue.IsEmpty && _scheduleQueue.IsEmpty;
    }

    //  TODO: Check if job can be instantiated via DI or argument-less constructor
    public void ScheduleJob(ScheduledJobData jobData)
    {
        _scheduleQueue.ScheduleJob(jobData);

        _configuration.Logger.LogInformation($"Scheduled job {jobData.Id} to run on {jobData.EnqueuedOn}");
    }

    public void ScheduleRecurrentJob(RecurrentJobData jobData)
    {
        jobData.EnqueuedOn = jobData.NextScheduleOn();
        _scheduleQueue.ScheduleJob(jobData);

        _configuration.Logger.LogInformation(
            $"Scheduled recurrent job {jobData.Id}. Next scheduled on {jobData.NextScheduleOn()}");
    }

    public bool CancelJob(ScheduledJobData jobData)
    {
        if (jobData.State == JobState.Scheduled)
            return _scheduleQueue.UnScheduleJob(jobData);

        return _waitQueue.DequeueJob(jobData);
    }

    public Snapshot TakeSnapshot()
    {
        return new Snapshot
        {
            SnapshotOn = DateTimeOffset.UtcNow,
            RunningJobs = _runningJobs.Select((kv, _) => ScheduledJobSnapshot.FromBaseJobData(kv.Value.JobData)),
            ScheduledJobs = _scheduleQueue.TakeSnapshot(),
            WaitingJobs = _waitQueue.TakeSnapshot()
        };
    }

    private void JobReady(object? _, JobReadyEventArgs eventArgs)
    {
        _waitQueue.EnqueueJob(eventArgs.JobData);

        _configuration.Logger.LogDebug($"Enqueued job {eventArgs.JobData.Id}");
    }

    private void JobStarted(object? _, JobFiredEventArgs e)
    {
        var jobTask = RunJobAsync(e.BaseJobData);

        _runningJobs[jobTask.Id] = new RunningJob(jobTask, e.BaseJobData);

        //  Removes the job from the running queue after it is completed
        jobTask.ContinueWith(_ => { _runningJobs.Remove(jobTask.Id, out var _); });
    }

    private async Task RunJobAsync(BaseJobData jobData)
    {
        //  If the job hasn't been instantiated, do it now
        if (!jobData.HasInstance())
        {
            var success = InstantiateJob(jobData);

            if (!success)
            {
                _configuration.Logger.LogCritical(
                    $"Could not run job {jobData.Id} : could not instantiate BaseJob.");
                return;
            }
        }

        try
        {
            _configuration.Logger.LogDebug($"Launching job {jobData.Id} ...");

            var jobInstance = jobData.Instance!;

            //  Job execution
            await jobInstance.BeforePerformAsync();
            await jobInstance.PerformAsync();

            jobData.State = JobState.Success;

            _configuration.Logger.LogDebug($"Job {jobData.Id} done !");

            RescheduleIfRecurrent(jobData);
        }
        catch (Exception e)
        {
            HandleFailingJob(jobData, e);
        }
        finally
        {
            _waitQueue.JobFinished();
        }
    }

    private void HandleFailingJob(BaseJobData jobData, Exception e)
    {
        _configuration.Logger.LogError(e, $"Job {jobData.Id} failure.");

        jobData.State = JobState.Failed;

        if (jobData.ShouldRetry) RetryFailingJob(jobData);
        else RescheduleIfRecurrent(jobData);

        //  For recurrent failing jobs, we re-schedule when the job cannot be retried
    }

    private void RetryFailingJob(BaseJobData jobData)
    {
        jobData.Retry();

        jobData.State = JobState.Enqueued;
        _waitQueue.EnqueueJob(jobData);

        _configuration.Logger.LogDebug(
            $"Retrying job {jobData.Id} ({jobData.RetryAttempts}/{jobData.MaxRetryAttempts}) ...");
    }

    private void RescheduleIfRecurrent(BaseJobData jobData)
    {
        if (jobData is RecurrentJobData recurrentJobData)
        {
            if (!recurrentJobData.HasNextSchedule())
            {
                _configuration.Logger.LogDebug($"Recurrent job {jobData.Id} has no next schedule");
                return;
            }

            recurrentJobData.EnqueuedOn = recurrentJobData.NextScheduleOn();
            recurrentJobData.Instance = null;

            _scheduleQueue.ScheduleJob(recurrentJobData);

            _configuration.Logger.LogInformation(
                $"Rescheduling recurrent job {jobData.Id} to run on {recurrentJobData.EnqueuedOn}");
        }
    }

    private bool InstantiateJob(BaseJobData jobData)
    {
        //  First, try to use DI
        if (_configuration.ServiceProvider is not null)
        {
            var jobService = _configuration.ServiceProvider.GetService(jobData.JobClass);

            if (jobService is not null)
            {
                jobData.Instance = (BaseJob)jobService;
                return true;
            }

            _configuration.Logger.LogDebug($"Could not find DI service for {jobData.JobClass} (job {jobData.Id})");
        }

        //  Try for a parameter-less instantiation
        if (!ArgumentLessConstructorChecker.HasArgumentLessConstructor(jobData.JobClass))
        {
            _configuration.Logger.LogError(
                $"Could not instantiate {jobData.JobClass} (job {jobData.Id}). Job class does not provide a parameter-less constructor");
            return false;
        }

        jobData.Instance = (BaseJob)Activator.CreateInstance(jobData.JobClass)!;
        return true;
    }

    private record RunningJob(Task Task, BaseJobData JobData);
}