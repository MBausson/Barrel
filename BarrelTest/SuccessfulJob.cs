namespace BarrelTest;

public class SuccessfulJob : TestJob
{
    protected override Task PerformAsync()
    {
        _ = Task.Delay(300).ContinueWith(_ => JobFinishedSource.SetResult(true));
        return Task.CompletedTask;
    }
}
