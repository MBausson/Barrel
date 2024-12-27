using Barrel;

namespace BarrelTest;

public abstract class TestJob : BaseJob
{
    //  This lets us signal the main thread (= the test) that we are done with the test.
    //  We could otherwise use some Task.Delay to wait jobs to finish, but this would be highly unreliable
    //  and would lead to a lot of flaky tests
    public readonly TaskCompletionSource<bool> JobFinishedSource = new();
    public readonly TaskCompletionSource<bool> JobRunningSource = new();

    protected override Task BeforePerformAsync()
    {
        JobRunningSource.SetResult(true);

        return Task.CompletedTask;
    }

    protected abstract override Task PerformAsync();
}
