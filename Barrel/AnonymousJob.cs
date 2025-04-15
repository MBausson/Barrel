namespace Barrel;

public class AnonymousJob(Action action) : BaseJob
{
    protected internal override Task PerformAsync()
    {
        return Task.Factory.StartNew(action);
    }
}
