using Barrel;

namespace BarrelTest;

public class BusyJob(int jobDurationMilliseconds = 500) : BaseJob
{
    protected override async Task PerformAsync()
    {
        await Task.Delay(jobDurationMilliseconds);
    }
}
