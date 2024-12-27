namespace Barrel.Scheduler;

public class ScheduleOptions
{
    public static readonly ScheduleOptions Default = new();

    /// <summary>
    /// Specifies a delay from which the job is executed. This delay takes act since the job gets scheduled.
    /// <remarks>Under certain circumstances (thread pool congestion, low priority...), the actual delay might increase</remarks>
    /// </summary>
    public TimeSpan Delay { get; private set; }

    /// <summary>
    /// <inheritdoc cref="BaseJob.JobPriority"/>
    /// </summary>
    public JobPriority Priority { get; private set; } = JobPriority.Default;

    /// <summary>
    /// <para> Creates a new ScheduleOptions with a delay. </para>
    /// <inheritdoc cref="Delay"/>
    /// </summary>
    public static ScheduleOptions FromDelay(TimeSpan delay) => new ScheduleOptions().WithDelay(delay);

    /// <summary>
    /// <para>Creates a new ScheduleOptions with a priority. </para>
    /// <inheritdoc cref="BaseJob.JobPriority"/>
    /// </summary>
    public static ScheduleOptions FromPriority(JobPriority priority) => new ScheduleOptions().WithPriority(priority);

    /// <summary>
    /// <inheritdoc cref="Delay"/>
    /// </summary>
    public ScheduleOptions WithDelay(TimeSpan delay)
    {
        Delay = delay;

        return this;
    }

    /// <summary>
    /// <inheritdoc cref="BaseJob.JobPriority" />
    /// </summary>
    public ScheduleOptions WithPriority(JobPriority priority)
    {
        Priority = priority;

        return this;
    }

    //  TODO: Make so we can schedule at a specific date
}
