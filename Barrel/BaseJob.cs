namespace Barrel;

public abstract class BaseJob
{
    /// <summary>
    ///     Hook method called before the execution of a job.
    /// </summary>
    protected internal virtual Task BeforePerformAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Method called during the execution of a job.
    /// </summary>
    protected internal abstract Task PerformAsync();
}
