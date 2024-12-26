using Barrel;

namespace BarrelTest;

public class BusyJob(int delay = 500) : BaseJob
{
    protected override async Task PerformAsync()
    {
        await Task.Delay(delay);
    }
}
