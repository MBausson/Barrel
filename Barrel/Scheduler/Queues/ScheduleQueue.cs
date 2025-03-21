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

    public void ScheduleJob(ScheduledJobData jobData)
    {
        jobData.JobState = JobState.Scheduled;

        lock (_queue)
        {
            _queue.Add(jobData.EnqueuedOn, jobData);
        }
    }

    private async Task ProcessSchedules()
    {
        while (!cancellationTokenSource.Token.IsCancellationRequested)
        {
            KeyValuePair<DateTime, BaseJobData>[] jobsToEnqueue;

            lock (_queue)
            {
                //  Improve this. Since this collection is sorted, we know dates that precede a future dates are also futures
                //  Thus we can stop iterating after a DateTime comparison fails
                //  We could also cache the result of DateTime.Now
                //  TODO
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
                OnJobReady?.Invoke(this, new JobReadyEventArgs(jobData));

                lock (_queue)
                {
                    _queue.Remove(date);
                }
            }
        }
    }
}