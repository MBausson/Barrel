using Barrel.JobData;

namespace Barrel.Scheduler.Queues;

public class JobReadyEventArgs(BaseJobData jobData) : EventArgs
{
    public BaseJobData JobData { get; } = jobData;
}

public class ScheduleQueue(int pollingRate, CancellationTokenSource cancellationTokenSource)
{
    private readonly SortedList<DateTime, BaseJobData> _queue = new();
    public bool IsEmpty => _queue.Count == 0;
    public event EventHandler<JobReadyEventArgs> OnJobReady = null!;

    public void StartProcessingSchedules()
    {
        _ = Task.Run(ProcessSchedules);
    }

    public void ScheduleJob(ScheduledBaseJobData baseJobData)
    {
        baseJobData.JobState = JobState.Scheduled;

        lock (_queue)
        {
            _queue.Add(baseJobData.EnqueuedOn, baseJobData);
        }
    }

    private async Task ProcessSchedules()
    {
        while (!cancellationTokenSource.Token.IsCancellationRequested)
        {
            KeyValuePair<DateTime, BaseJobData>[] jobsToEnqueue;

            lock (_queue)
            {
                jobsToEnqueue = _queue
                    .Where(kv => kv.Key <= DateTime.Now)
                    .ToArray();
            }

            if (jobsToEnqueue.Length == 0)
            {
                await Task.Delay(pollingRate);
                continue;
            }

            foreach (var (date, jobData) in jobsToEnqueue)
            {
                lock (_queue)
                {
                    _queue.Remove(date);
                }

                OnJobReady?.Invoke(this, new JobReadyEventArgs(jobData));
            }
        }
    }
}
