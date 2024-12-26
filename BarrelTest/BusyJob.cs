using Barrel;

namespace BarrelTest;

public class BusyJob(TaskCompletionSource<bool> completionSource, int jobDurationMilliseconds = 600)
    : TestJob(completionSource)
{
    protected override async Task PerformAsync()
    {
        await Task.Delay(jobDurationMilliseconds);
    }
}
