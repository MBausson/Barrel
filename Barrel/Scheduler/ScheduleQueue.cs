using System.Collections.Concurrent;

namespace Barrel.Scheduler;

public class JobReadyEventArgs(ScheduledJobData jobData) : EventArgs
{
    public ScheduledJobData JobData { get; } = jobData;
}

public class ScheduleQueue(int pollingRate, CancellationTokenSource cancellationTokenSource)
{
    public event EventHandler<JobReadyEventArgs> OnJobReady = null!;
    public bool IsEmpty => _queue.Count == 0;

    private readonly SortedList<DateTime, ScheduledJobData> _queue = new();

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
            KeyValuePair<DateTime, ScheduledJobData>[] jobsToEnqueue;

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

                OnJobReady?.Invoke(this, new(jobData));
            }
        }
    }
}
