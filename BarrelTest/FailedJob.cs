using Barrel;

namespace BarrelTest;

public class FailedJob : BaseJob
{
    protected override Task PerformAsync()
    {
        throw new Exception("Predictable job exception");
    }
}
