namespace BarrelTest;

public class TestJob : BaseJob
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

    protected override Task PerformAsync()
    {
        //  We declare this job as done with CompletionSource
        //  But the scheduler might not have caught the error yet, so we delay it.
        //  For the test -> The job is done
        //  For the scheduler -> The job is still running
        _ = Task.Delay(400).ContinueWith(_ => JobFinishedSource.SetResult(true));

        return Task.CompletedTask;
    }
}