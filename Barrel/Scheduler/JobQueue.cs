using System.Collections.Concurrent;

namespace Barrel.Scheduler;

internal class JobFiredEventArgs(ScheduledJobData jobData) : EventArgs
{
    public readonly ScheduledJobData JobData = jobData;
}

internal class JobQueue(int pollingRate, int maxConcurrentJobs, CancellationTokenSource cancellationTokenSource)
{
    public event EventHandler<JobFiredEventArgs> OnJobFired = null!;
    public bool IsEmpty => _queue.IsEmpty;

    private readonly ConcurrentQueue<ScheduledJobData> _queue = new();
    private readonly int _pollingRate = pollingRate;
    private readonly CancellationTokenSource _cancellationTokenSource = cancellationTokenSource;
    private readonly SemaphoreSlim _semaphore = new(maxConcurrentJobs);

    public void StartProcessJobs()
    {
        _ = Task.Run(ProcessJobs);
    }

    public void EnqueueJob(ScheduledJobData jobData)
    {
        _queue.Enqueue(jobData);
        jobData.JobState = JobState.Enqueued;
    }

    //  Called when a job finished its work
    public void JobFinished() => _semaphore.Release();

    //  Responsible for launching jobs as soon as the number of concurrent jobs is not exceeding its maximum
    private async Task ProcessJobs()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            if (!_queue.TryDequeue(out var jobData))
            {
                await Task.Delay(_pollingRate);
                continue;
            }

            await _semaphore.WaitAsync();

            jobData.JobState = JobState.Running;

            //  If the job hasn't been instantiated, do it now
            if (!jobData.HasInstance()) jobData.InstantiateJob();

            OnJobFired?.Invoke(this, new(jobData));
        }
    }
}
