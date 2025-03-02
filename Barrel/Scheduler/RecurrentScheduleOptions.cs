// ReSharper disable All
namespace Barrel.Scheduler;

public class RecurrentScheduleOptions : ScheduleOptions
{
    /// <summary>
    /// Represents the amount of time between each execution of a recurrent job.  
    /// <remarks>The delay must be greater or equal to 1 second</remarks>
    /// </summary>
    public TimeSpan Periodicity { get; private set; }

    /// <inheritdoc cref="Periodicity" />
    public RecurrentScheduleOptions Every(TimeSpan periodicity)
    {
        if (periodicity.TotalSeconds < 1) throw new ArgumentOutOfRangeException("Periodicity must be at least one second long.");

        Periodicity = periodicity;

        return this;
    }
}
