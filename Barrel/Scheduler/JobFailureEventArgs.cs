namespace Barrel.Scheduler;

public class JobFailureEventArgs : EventArgs
{
    public required BaseJob Job { get; init; }
    public required Exception Exception { get; init; }
}
