namespace BarrelTest;

public class FailedJob(TaskCompletionSource<bool> completionSource) : TestJob(completionSource)
{
    protected override Task PerformAsync()
    {
        //  We declare this job as done with CompletionSource
        //  But the scheduler might not have caught the error yet.
        //  For the test -> The job is done
        //  For the scheduler -> The job is still runnning

        //  I'm thinking of adding this behaviour to other test jobs...
        Task.Delay(150).ContinueWith(_ => CompletionSource.SetResult(true));

        throw new Exception("Predictable job exception");
    }
}
