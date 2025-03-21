using System.Collections.Concurrent;
using Barrel.JobData;

namespace Barrel.Scheduler.Queues;

internal class JobFiredEventArgs(BaseJobData jobData) : EventArgs
{
    public readonly BaseJobData BaseJobData = jobData;
}

internal class JobQueue(int pollingRate, int maxConcurrentJobs, CancellationTokenSource cancellationTokenSource)
{
    private readonly ConcurrentQueue<BaseJobData> _queue = new();
    private readonly SemaphoreSlim _semaphore = new(maxConcurrentJobs);
    public bool IsEmpty => _queue.IsEmpty;
    public event EventHandler<JobFiredEventArgs> OnJobFired = null!;

    public void StartProcessingJobs()
    {
        _ = Task.Run(ProcessJobs);
    }

    public void EnqueueJob(BaseJobData jobData)
    {
        _queue.Enqueue(jobData);
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
            if (!_queue.TryPeek(out var jobData))
            {
                await Task.Delay(pollingRate, cancellationTokenSource.Token);
                continue;
            }

            await _semaphore.WaitAsync(cancellationTokenSource.Token);

            jobData.JobState = JobState.Running;

            //  If the job hasn't been instantiated, do it now
            if (!jobData.HasInstance()) jobData.InstantiateJob();

            OnJobFired?.Invoke(this, new JobFiredEventArgs(jobData));

            //  Dequeues after firing the job execution
            _queue.TryDequeue(out _);
        }
    }
}
