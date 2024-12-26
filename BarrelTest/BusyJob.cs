namespace BarrelTest;

public class BusyJob(TaskCompletionSource<bool> completionSource, int jobDurationMilliseconds = 600)
    : TestJob(completionSource)
{
    protected override async Task PerformAsync()
    {
        await Task.Delay(jobDurationMilliseconds);
        _ = Task.Delay(150).ContinueWith(_ => CompletionSource.SetResult(true));
    }
}
