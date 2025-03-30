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
            var jobsToEnqueue = FindJobsToEnqueue();

            if (jobsToEnqueue.Count == 0)
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

    private List<KeyValuePair<DateTime, BaseJobData>> FindJobsToEnqueue()
    {
        List<KeyValuePair<DateTime, BaseJobData>> jobsToEnqueue = [];

        lock (_queue)
        {
            var dateNow = DateTime.Now;

            foreach (var (scheduleDateTime, jobData) in _queue)
            {
                //  The _queue list is sorted, thus there is no need to look further if this check fails
                if (scheduleDateTime > dateNow) break;

                jobsToEnqueue.Add(new KeyValuePair<DateTime, BaseJobData>(scheduleDateTime, jobData));
            }
        }

        return jobsToEnqueue;
    }
}
