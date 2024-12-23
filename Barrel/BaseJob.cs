namespace Barrel;

public abstract class BaseJob
{
    protected internal virtual void BeforeSchedule() { }

    protected internal abstract void Perform();
}
