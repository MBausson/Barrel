using Barrel;

namespace BarrelTest;

public abstract class TestJob(TaskCompletionSource<bool> completionSource) : BaseJob
{
    //  This lets us signal the main thread (= the test) that we are done with the test.
    //  We could otherwise use some Task.Delay to wait jobs to finish, but this would be highly unreliable
    //  and would lead to a lot of flaky tests
    protected TaskCompletionSource<bool> CompletionSource = completionSource;

    protected abstract override Task PerformAsync();
}
