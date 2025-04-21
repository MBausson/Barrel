using Barrel.JobData;

namespace Barrel.Scheduler.Queues;

internal class JobFiredEventArgs(BaseJobData jobData) : EventArgs
{
    public readonly BaseJobData BaseJobData = jobData;
}

internal class WaitQueue(int pollingRate, int maxConcurrentJobs, CancellationTokenSource cancellationTokenSource)
{
    private readonly List<BaseJobData> _queue = new();
    private readonly SemaphoreSlim _semaphore = new(maxConcurrentJobs);
    public bool IsEmpty => _queue.Count == 0;
    public event EventHandler<JobFiredEventArgs> OnJobFired = null!;

    public void StartProcessingJobs()
    {
        _ = Task.Run(ProcessJobs);
    }

    public void EnqueueJob(BaseJobData jobData)
    {
        lock (_queue)
        {
            _queue.Add(jobData);
        }

        jobData.State = JobState.Enqueued;
    }

    public bool DequeueJob(BaseJobData jobData)
    {
        lock (_queue)
        {
            return _queue.Remove(jobData);
        }
    }

    //  Called when a job finished its work
    public void JobFinished()
    {
        _semaphore.Release();
    }

    public IEnumerable<ScheduledJobSnapshot> TakeSnapshot()
    {
        return _queue.Select(ScheduledJobSnapshot.FromBaseJobData);
    }

    //  Responsible for launching jobs as soon as the number of concurrent jobs is not exceeding its maximum
    private async Task ProcessJobs()
    {
        while (!cancellationTokenSource.Token.IsCancellationRequested)
        {
            if (!TryGetJob(out var jobData))
            {
                await Task.Delay(pollingRate, cancellationTokenSource.Token);
                continue;
            }

            await _semaphore.WaitAsync(cancellationTokenSource.Token);

            jobData.State = JobState.Running;

            OnJobFired?.Invoke(this, new JobFiredEventArgs(jobData));

            //  Dequeues after firing the job execution
            _queue.Remove(jobData);
        }
    }

    private bool TryGetJob(out BaseJobData job)
    {
        if (IsEmpty)
        {
            job = default!;
            return false;
        }

        job = _queue.First();

        for (var i = 1; i < _queue.Count; i++)
        {
            //  No need to search for jobs with higher priority if we're already on a High priority
            if (job.Priority == JobPriority.High) break;

            var nextJob = _queue[i];

            if (nextJob.Priority > job.Priority) job = nextJob;
        }

        return true;
    }
}
