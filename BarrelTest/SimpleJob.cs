using Barrel;

namespace BarrelTest;

public class SimpleJob : BaseJob
{
    protected override Task PerformAsync()
    {
        return Task.CompletedTask;
    }
}
