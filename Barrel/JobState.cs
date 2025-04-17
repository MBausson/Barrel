namespace Barrel;

public enum JobState
{
    /// <summary>
    ///     State of a job that has not been scheduled yet
    /// </summary>
    NotStarted,

    /// <summary>
    ///     State of a job that is waiting for its delay to run out.
    /// </summary>
    Scheduled,

    /// <summary>
    ///     State of a job that is waiting to be executed, due to a job pool congestion.
    /// </summary>
    Enqueued,

    /// <summary>
    ///     State of a job that is currently being executed
    /// </summary>
    Running,

    /// <summary>
    ///     State of a job that has been successfully executed
    /// </summary>
    Success,

    /// <summary>
    ///     State of a job that whose execution has failed, due to an unexpected exception.
    /// </summary>
    Failed,

    /// <summary>
    /// State of a job that has been cancelled before its execution began.
    /// </summary>
    Cancelled
}
