using Barrel;

namespace BarrelTest;

public class SuccessfulJob(TaskCompletionSource<bool> completionSource) : TestJob(completionSource)
{
    protected override Task PerformAsync()
    {
        _ = Task.Delay(150).ContinueWith(_ => CompletionSource.SetResult(true));
        return Task.CompletedTask;
    }
}
