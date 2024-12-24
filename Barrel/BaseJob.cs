namespace Barrel;

public abstract class BaseJob
{
    protected internal readonly Guid JobId = Guid.NewGuid();

    protected internal virtual void BeforeSchedule() { }

    protected internal abstract void Perform();
}
