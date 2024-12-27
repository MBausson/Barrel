namespace BarrelTest;

public class BusyJob : TestJob
{
    private readonly int _jobDurationMilliseconds = 600;

    public BusyJob()
    {

    }

    public BusyJob(int jobDurationMilliseconds)
    {
        if (jobDurationMilliseconds < 0) throw new ArgumentOutOfRangeException(nameof(jobDurationMilliseconds));

        _jobDurationMilliseconds = jobDurationMilliseconds;
    }

    protected override async Task PerformAsync()
    {
        await Task.Delay(_jobDurationMilliseconds);
        _ = Task.Delay(300).ContinueWith(_ => JobFinishedSource.SetResult(true));
    }
}
