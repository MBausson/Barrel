using Barrel.JobData;

namespace Barrel;

/// <summary>
/// Represents the state of the different internal queues of Barrel at a given instant.
/// </summary>
public readonly record struct Snapshot
{
    public DateTimeOffset SnapshotOn { get; init; }

    public IEnumerable<ScheduledJobSnapshot> RunningJobs { get; init; }
    public IEnumerable<ScheduledJobSnapshot> WaitingJobs { get; init; }
    public IEnumerable<ScheduledJobSnapshot> ScheduledJobs { get; init; }

    public IEnumerable<ScheduledJobSnapshot> AllJobs() => WaitingJobs.Concat(ScheduledJobs);
}

/// <summary>
/// Represents the state of a scheduled job a given instant.
/// </summary>
public record struct ScheduledJobSnapshot
{
    public Guid JobId { get; init; }
    public JobState State { get; init; }
    public JobPriority Priority { get; init; }
    public int MaxRetryAttempts { get; init; }
    public int RetryAttempts { get; init; }
    public Type JobClass { get; init; }
    public DateTimeOffset NextScheduleOn { get; init; }

    internal static ScheduledJobSnapshot FromBaseJobData(BaseJobData jobData)
    {
        return new ScheduledJobSnapshot
        {
            JobId = jobData.Id,
            State = jobData.State,
            Priority = jobData.Priority,
            MaxRetryAttempts = jobData.MaxRetryAttempts,
            RetryAttempts = jobData.RetryAttempts,
            JobClass = jobData.JobClass,
            NextScheduleOn = jobData.NextScheduleOn()
        };
    }
}
