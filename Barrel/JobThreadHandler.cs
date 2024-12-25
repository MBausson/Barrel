using System.Collections.Concurrent;
using Barrel.Configuration;

namespace Barrel;

internal class JobThreadHandler : IDisposable
{
    private readonly JobSchedulerConfiguration _configuration;
    //  The semaphore ensures that we aren't using more threads than we should
    private readonly SemaphoreSlim _semaphore;
    //  Queues of job to run. These jobs should be ran ASAP, according to their priority
    private readonly ConcurrentQueue<(BaseJob job, TimeSpan delay)> _jobQueue;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public JobThreadHandler(JobSchedulerConfiguration configuration)
    {
        _configuration = configuration;

        _semaphore = new SemaphoreSlim(_configuration.MaxThreads);
        _jobQueue = new();
        _cancellationTokenSource = new();

        _ = Task.Run(ProcessQueue);
    }

    public void EnqueueJob(BaseJob job, TimeSpan delay)
    {
        job.JobState = JobState.Enqueued;
        _jobQueue.Enqueue((job, delay));
    }

    private async Task ProcessQueue()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            if (!_jobQueue.TryDequeue(out (BaseJob job, TimeSpan delay) item))
            {
                await Task.Delay(_configuration.QueuePollingRate);
                continue;
            }

            await _semaphore.WaitAsync();

            item.job.JobState = JobState.Running;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(item.delay);
                    item.job.Perform();
                    item.job.JobState = JobState.Running;
                }
                catch (Exception)
                {
                    //  TODO: Rethrow the error to the main thread.
                    item.job.JobState = JobState.Failed;
                    throw;
                }
                finally
                {
                    _semaphore.Release();
                }
            });
        }
    }

    public void Dispose()
    {
        _semaphore.Dispose();
        _cancellationTokenSource.Dispose();
    }
}
