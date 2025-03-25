using System.Collections.Concurrent;
using Barrel.JobData;

namespace Barrel.Scheduler.Queues;

internal class JobFiredEventArgs(BaseJobData jobData) : EventArgs
{
    public readonly BaseJobData BaseJobData = jobData;
}

internal class JobQueue(int pollingRate, int maxConcurrentJobs, CancellationTokenSource cancellationTokenSource)
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

        jobData.JobState = JobState.Enqueued;
    }

    //  Called when a job finished its work
    public void JobFinished()
    {
        _semaphore.Release();
    }

    //  Responsible for launching jobs as soon as the number of concurrent jobs is not exceeding its maximum
    private async Task ProcessJobs()
    {
        while (!cancellationTokenSource.Token.IsCancellationRequested)
        {
            if (IsEmpty)
            {
                await Task.Delay(pollingRate, cancellationTokenSource.Token);
                continue;
            }

            var jobData = _queue.First();

            await _semaphore.WaitAsync(cancellationTokenSource.Token);

            jobData.JobState = JobState.Running;

            //  If the job hasn't been instantiated, do it now
            if (!jobData.HasInstance()) jobData.InstantiateJob();

            OnJobFired?.Invoke(this, new JobFiredEventArgs(jobData));

            //  Dequeues after firing the job execution
            _queue.Remove(jobData);
        }
    }
}
