using Barrel;

namespace BarrelTest;

public class SuccessfulJob : BaseJob
{
    protected override Task PerformAsync()
    {
        return Task.CompletedTask;
    }
}
