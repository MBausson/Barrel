namespace Barrel.Scheduler;

public class ScheduleOptions
{
    public static readonly ScheduleOptions Default = new();

    /// <summary>
    ///     Specifies a delay from which the job is executed. This delay takes act since the job gets scheduled.
    ///     <remarks>Under certain circumstances (jobs pool congestion, low priority...), the actual delay might increase</remarks>
    /// </summary>
    public TimeSpan Delay { get; private set; }

    /// <summary>
    ///     <inheritdoc cref="BaseJob.JobPriority" />
    /// </summary>
    public JobPriority Priority { get; private set; } = JobPriority.Default;

    /// <summary>
    ///     Specifies the maximum amount of times a job can be retried if it failed before.
    /// </summary>
    public int MaxRetries { get; private set; }

    /// <summary>
    ///     <para> Creates a new ScheduleOptions with a delay. </para>
    ///     <inheritdoc cref="Delay" />
    /// </summary>
    public static ScheduleOptions FromDelay(TimeSpan delay)
    {
        return new ScheduleOptions().WithDelay(delay);
    }

    /// <summary>
    ///     <para>Creates a new ScheduleOptions with a priority. </para>
    ///     <inheritdoc cref="BaseJob.JobPriority" />
    /// </summary>
    public static ScheduleOptions FromPriority(JobPriority priority)
    {
        return new ScheduleOptions().WithPriority(priority);
    }

    /// <summary>
    ///     <inheritdoc cref="Delay" />
    /// </summary>
    public ScheduleOptions WithDelay(TimeSpan delay)
    {
        Delay = delay;

        return this;
    }

    /// <summary>
    ///     <inheritdoc cref="BaseJob.JobPriority" />
    /// </summary>
    public ScheduleOptions WithPriority(JobPriority priority)
    {
        Priority = priority;

        return this;
    }

    public ScheduleOptions WithMaxRetries(int maxRetries)
    {
        if (maxRetries < 0) throw new ArgumentOutOfRangeException($"{nameof(MaxRetries)} should be positive or zero.");

        MaxRetries = maxRetries;

        return this;
    }

    public virtual DateTime NextScheduleOn()
    {
        return DateTime.Now + Delay;
    }
}