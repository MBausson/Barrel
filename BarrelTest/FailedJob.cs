namespace BarrelTest;

public class FailedJob : TestJob
{
    protected override Task PerformAsync()
    {
        //  We declare this job as done with CompletionSource
        //  But the scheduler might not have caught the error yet.
        //  For the test -> The job is done
        //  For the scheduler -> The job is still runnning
        Task.Delay(300).ContinueWith(_ => JobFinishedSource.SetResult(true));

        throw new Exception("Predictable job exception");
    }
}
