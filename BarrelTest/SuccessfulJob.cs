using Barrel;

namespace BarrelTest;

public class SuccessfulJob(TaskCompletionSource<bool> completionSource) : TestJob(completionSource)
{
    protected override Task PerformAsync()
    {
        CompletionSource.SetResult(true);
        return Task.CompletedTask;
    }
}
